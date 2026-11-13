using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using MlSeed;

try
{
    var apply = args.Contains("--apply", StringComparer.Ordinal);
    var appHost = args.Contains("--apphost", StringComparer.Ordinal);
    var export = args.Contains("--export", StringComparer.Ordinal);
    var options = new Dictionary<string, string>(StringComparer.Ordinal);
    for (var i = 0; i < args.Length; i++)
    {
        if (args[i] is "--apply" or "--apphost" or "--export") continue;
        if (args[i] is "--count" or "--seed" or "--end-date" or "--output" && i + 1 < args.Length)
        {
            var key = args[i];
            options.Add(key, args[++i]);
            continue;
        }
        throw new ArgumentException("Usage: MlSeed [--count 1000] [--seed 42] [--end-date 2026-11-01] [--apply] [--apphost], or MlSeed --export [--apphost] [--output FILE.csv]");
    }

    if (export)
    {
        if (apply || options.Keys.Any(key => key != "--output"))
            throw new ArgumentException("Export cannot be combined with seeding options.");
        var output = options.GetValueOrDefault("--output") ?? Environment.GetEnvironmentVariable("ML_EXPORT_OUTPUT")
            ?? Path.Combine("artifacts", "ml", "repair-orders.csv");
        var exportConnection = appHost ? ReadAppHostConnection("OrdersDb") : ReadIsolatedConnection();
        exportConnection.InitialCatalog = appHost ? "OrdersDb" : "OrdersMlSeed";
        await RepairDatasetExport.WriteAsync(exportConnection, output);
        return 0;
    }
    if (options.ContainsKey("--output"))
        throw new ArgumentException("--output is only available with --export.");

    var count = int.Parse(options.GetValueOrDefault("--count", "1000"), CultureInfo.InvariantCulture);
    var seed = int.Parse(options.GetValueOrDefault("--seed", "42"), CultureInfo.InvariantCulture);
    var endDate = DateOnly.ParseExact(options.GetValueOrDefault("--end-date", "2026-11-01"), "yyyy-MM-dd",
        CultureInfo.InvariantCulture);
    if (count is < 1 or > 3000 || endDate.Year is < 2010 or > 2099 ||
        endDate >= DateOnly.FromDateTime(DateTime.UtcNow))
        throw new ArgumentException("Choose 1–3000 orders and a past end date from 2010 through 2099.");

    var data = SeedData.Generate(count, seed, endDate);
    Validate(data);
    var delayed = data.Repairs.Count(order => order.CompletedAt > order.PlannedAt);
    Console.WriteLine($"Categories: {SeedData.Catalog.Select(x => x.Category).Distinct().Count()}, services: {SeedData.Catalog.Length}");
    Console.WriteLine($"Customers: {data.Customers.Length}, mechanics: {data.Mechanics.Length}, vehicles: {data.Vehicles.Length}");
    Console.WriteLine($"Completed orders: {count}, order items: {data.Repairs.Sum(x => x.Services.Length)}, delayed: {delayed} ({(double)delayed / count:P1})");
    Console.WriteLine($"First and last appointments: {data.PeriodStart:yyyy-MM-dd} / {data.Repairs[^1].Slot:yyyy-MM-dd}");
    Console.WriteLine($"Dataset SHA-256: {Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(data)))}");

    if (!apply)
    {
        Console.WriteLine("Preview only. No database connection was made.");
        return 0;
    }

    var connection = appHost ? ReadAppHostConnection("OrdersDb", "CatalogDb", "UsersDb", "VehiclesDb") : ReadIsolatedConnection();
    Console.WriteLine(appHost
        ? "Target: AppHost (OrdersDb, CatalogDb, UsersDb, VehiclesDb)."
        : "Target: isolated Compose databases (*MlSeed).");
    var demoPassword = Environment.GetEnvironmentVariable("ML_SEED_DEMO_PASSWORD");
    var applied = await SqlSeed.ApplyAsync(connection, appHost, data, count, seed, endDate, demoPassword);
    Console.WriteLine(applied ? "Seed batch saved." : "This batch already exists; no rows were added.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static SqlConnectionStringBuilder ReadIsolatedConnection()
{
    var password = Environment.GetEnvironmentVariable("ML_SEED_SQL_PASSWORD");
    if (string.IsNullOrWhiteSpace(password))
        throw new ArgumentException("Set ML_SEED_SQL_PASSWORD for the isolated Compose SQL Server.");
    return new SqlConnectionStringBuilder
    {
        DataSource = "127.0.0.1,11433",
        UserID = "sa",
        Password = password,
        TrustServerCertificate = true,
        ConnectTimeout = 10
    };
}

static SqlConnectionStringBuilder ReadAppHostConnection(params string[] databases)
{
    SqlConnectionStringBuilder? ordersConnection = null;
    foreach (var database in databases)
    {
        var value = Environment.GetEnvironmentVariable($"ConnectionStrings__{database}");
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Start ml-seed or ml-export from the Aspire dashboard so AppHost can supply the database connection.");

        var connection = new SqlConnectionStringBuilder(value);
        var host = connection.DataSource.Split(',')[0].Trim().ToLowerInvariant();
        if (host.StartsWith("tcp:", StringComparison.Ordinal)) host = host[4..];
        if (connection.InitialCatalog != database || host is not ("localhost" or "127.0.0.1" or "[::1]" or "::1") ||
            (ordersConnection is not null && !string.Equals(connection.DataSource, ordersConnection.DataSource, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("AppHost ML tools require the expected development databases on the same local SQL Server.");

        ordersConnection ??= connection;
    }
    return ordersConnection!;
}

static void Validate(SeedData data)
{
    var shopZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
    if (data.Repairs.Select(x => x.Slot).Distinct().Count() != data.Repairs.Length ||
        data.Repairs.Any(x => x.Services.Length == 0 || x.Services.Distinct().Count() != x.Services.Length ||
            x.Services.Any(service => !SeedData.Catalog.Contains(service)) ||
            x.PlannedAt <= TimeZoneInfo.ConvertTimeToUtc(x.Slot, shopZone) ||
            x.StartedAt < TimeZoneInfo.ConvertTimeToUtc(x.Slot, shopZone) ||
            x.CompletedAt <= x.StartedAt || x.MileageKm < 0 ||
            !data.Customers.Any(user => user.Id == x.Vehicle.OwnerId)))
        throw new InvalidOperationException("Invalid order, service, vehicle ownership or timestamps in generated data.");

    foreach (var group in data.Repairs.GroupBy(x => x.Mechanic.Id))
    {
        var availableAt = DateTime.MinValue;
        foreach (var repair in group.OrderBy(x => x.StartedAt))
        {
            if (repair.StartedAt < availableAt)
                throw new InvalidOperationException("Mechanic has overlapping repairs.");
            availableAt = repair.CompletedAt;
        }
    }
}
