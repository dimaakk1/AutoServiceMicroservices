using System.Globalization;
using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;

namespace MlSeed;

internal static class RepairDatasetExport
{
    private const string Header = "order_id,appointment_local,mechanic_id,vehicle_year,mileage_km,service_ids,service_count,planned_budget_minutes,duration_minutes,delayed";
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    internal static async Task WriteAsync(SqlConnectionStringBuilder settings, string output)
    {
        var outputPath = Path.GetFullPath(output);
        if (!string.Equals(Path.GetExtension(outputPath), ".csv", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The export output must be a .csv file.");

        await using var db = new SqlConnection(settings.ConnectionString);
        await db.OpenAsync();
        var ready = await db.ExecuteScalarAsync<int>("""
            SELECT CASE WHEN OBJECT_ID(N'dbo.MlSeedBatch', N'U') IS NOT NULL
                AND OBJECT_ID(N'dbo.MlSeedVehicleSnapshots', N'U') IS NOT NULL THEN 1 ELSE 0 END;
            """);
        if (ready != 1)
            throw new InvalidOperationException("No ML seed batch was found in this database. Run ml-seed first.");

        var configuration = await db.QuerySingleOrDefaultAsync<string>(
            "SELECT Configuration FROM dbo.MlSeedBatch WHERE Id = 1;");
        var fields = configuration?.Split(':');
        if (fields is not { Length: 4 } || fields[0] != "v1" ||
            !int.TryParse(fields[1], NumberStyles.None, Invariant, out var expected) || expected < 1)
            throw new InvalidOperationException("The ML seed batch marker is missing or unsupported.");

        var rows = await db.QueryAsync<SourceRow>("""
            SELECT o.OrderId, o.OrderDate, o.Status, o.MechanicId,
                   snapshot.VehicleYear, snapshot.MileageKm, o.PlannedCompletionAt,
                   o.WorkStartedAt, o.WorkCompletedAt, i.ProductId, i.Quantity
            FROM dbo.MlSeedVehicleSnapshots AS snapshot
            JOIN dbo.Orders AS o ON o.OrderId = snapshot.OrderId
            LEFT JOIN dbo.OrderItems AS i ON i.OrderId = o.OrderId
            ORDER BY o.OrderDate, o.OrderId, i.OrderItemId;
            """);
        var orders = rows.GroupBy(row => row.OrderId).ToArray();
        if (orders.Length != expected)
            throw new InvalidOperationException($"Expected {expected} seeded orders but found {orders.Length}. No CSV was written.");

        var shopZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        var csv = new StringBuilder().Append(Header).Append('\n');
        var delayedCount = 0;
        foreach (var order in orders)
        {
            var row = order.First();
            if (row.Status != "Completed" || string.IsNullOrWhiteSpace(row.MechanicId) ||
                row.OrderDate is not { } appointment || row.WorkStartedAt is not { } started ||
                row.WorkCompletedAt is not { } completed || row.PlannedCompletionAt is not { } planned ||
                completed <= started || row.MileageKm < 0 || row.VehicleYear > appointment.Year ||
                order.Any(item => item.ProductId is null or <= 0 || item.Quantity is null or <= 0))
                throw new InvalidOperationException($"Seeded order {order.Key} has missing or invalid data. No CSV was written.");

            var appointmentUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(appointment, DateTimeKind.Unspecified), shopZone);
            if (planned <= appointmentUtc || started < appointmentUtc)
                throw new InvalidOperationException($"Seeded order {order.Key} has invalid timestamps. No CSV was written.");

            var delayed = completed > planned;
            if (delayed) delayedCount++;
            var serviceIds = JsonSerializer.Serialize(order.Select(item => item.ProductId!.Value).Distinct().Order());
            csv.AppendJoin(',', new[]
            {
                row.OrderId.ToString(Invariant),
                appointment.ToString("yyyy-MM-dd'T'HH:mm:ss", Invariant),
                Escape(row.MechanicId),
                row.VehicleYear.ToString(Invariant),
                row.MileageKm.ToString(Invariant),
                Escape(serviceIds),
                order.Sum(item => item.Quantity!.Value).ToString(Invariant),
                (planned - appointmentUtc).TotalMinutes.ToString("0.######", Invariant),
                (completed - started).TotalMinutes.ToString("0.######", Invariant),
                delayed ? "1" : "0"
            }).Append('\n');
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var temporary = outputPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporary, csv.ToString(), new UTF8Encoding(false));
            File.Move(temporary, outputPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }

        Console.WriteLine($"Exported {orders.Length} orders ({delayedCount} delayed) from {settings.InitialCatalog}.");
        Console.WriteLine($"CSV: {outputPath}");
    }

    private static string Escape(string value) => $"\"{value.Replace("\"", "\"\"")}\"";

    private sealed class SourceRow
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? Status { get; set; }
        public string? MechanicId { get; set; }
        public int VehicleYear { get; set; }
        public int MileageKm { get; set; }
        public DateTime? PlannedCompletionAt { get; set; }
        public DateTime? WorkStartedAt { get; set; }
        public DateTime? WorkCompletedAt { get; set; }
        public int? ProductId { get; set; }
        public int? Quantity { get; set; }
    }
}
