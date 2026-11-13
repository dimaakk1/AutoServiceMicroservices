using AutoServiceCatalog.DAL.Db;
using AutoServiceUsers.DAL.DB;
using AutoServiceUsers.DAL.Entities;
using AutoserviceOrders.DAL.db;
using AutoserviceVehicle.DAL.DB;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MlSeed;

internal static class SqlSeed
{
    private static readonly string[] Suppliers = ["Bosch", "Continental", "Denso", "SKF", "Valeo"];

    internal static async Task<bool> ApplyAsync(SqlConnectionStringBuilder server, bool appHost, SeedData data,
        int count, int seed, DateOnly endDate, string? demoPassword)
    {
        var ordersDatabase = appHost ? "OrdersDb" : "OrdersMlSeed";
        var catalogDatabase = appHost ? "CatalogDb" : "CatalogMlSeed";
        var usersDatabase = appHost ? "UsersDb" : "UsersMlSeed";
        var vehiclesDatabase = appHost ? "VehiclesDb" : "VehiclesMlSeed";
        string ConnectionFor(string database) => new SqlConnectionStringBuilder(server.ConnectionString)
            { InitialCatalog = database }.ConnectionString;

        if (!appHost)
        {
            await using var catalog = new CarServiceContext(new DbContextOptionsBuilder<CarServiceContext>()
                .UseSqlServer(ConnectionFor(catalogDatabase)).Options);
            await using var users = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(ConnectionFor(usersDatabase)).Options);
            await using var vehicles = new VehicleDbContext(new DbContextOptionsBuilder<VehicleDbContext>()
                .UseSqlServer(ConnectionFor(vehiclesDatabase)).Options);
            await catalog.Database.MigrateAsync();
            await users.Database.MigrateAsync();
            await vehicles.Database.MigrateAsync();
            DatabaseInitializer.Initialize(ConnectionFor(ordersDatabase));
        }

        await using var db = new SqlConnection(ConnectionFor(ordersDatabase));
        await db.OpenAsync();
        await using var transaction = await db.BeginTransactionAsync();
        Task<int> Execute(string sql, object? parameters = null) => db.ExecuteAsync(sql, parameters, transaction);
        Task<T> Scalar<T>(string sql, object? parameters = null) => db.ExecuteScalarAsync<T>(sql, parameters, transaction)!;

        var lockResult = await Scalar<int>("""
            DECLARE @Result int;
            EXEC @Result = sys.sp_getapplock @Resource=N'AutoService.MlSeed', @LockMode='Exclusive', @LockTimeout=10000;
            SELECT @Result;
            """);
        if (lockResult < 0)
            throw new InvalidOperationException("Could not lock the test seed batch. Try again later.");

        if (appHost && await Scalar<int>("""
            SELECT CASE WHEN COL_LENGTH('dbo.Orders', 'PlannedCompletionAt') IS NOT NULL
                AND OBJECT_ID('dbo.OrderItems','U') IS NOT NULL
                AND OBJECT_ID('CatalogDb.dbo.Services','U') IS NOT NULL
                AND OBJECT_ID('UsersDb.dbo.AspNetUsers','U') IS NOT NULL
                AND OBJECT_ID('VehiclesDb.dbo.Vehicles','U') IS NOT NULL THEN 1 ELSE 0 END;
            """) != 1)
            throw new InvalidOperationException("AppHost databases are not initialized yet. Wait for the four services to start and retry.");

        await Execute("""
            IF OBJECT_ID(N'dbo.MlSeedBatch', N'U') IS NULL
                CREATE TABLE dbo.MlSeedBatch (Id int NOT NULL PRIMARY KEY, Configuration nvarchar(120) NOT NULL);
            """);
        var configuration = FormattableString.Invariant($"v1:{count}:{seed}:{endDate:yyyy-MM-dd}");
        async Task VerifyStoredBatch()
        {
            var orders = await Scalar<int>($"""
                SELECT COUNT(*) FROM dbo.MlSeedVehicleSnapshots AS snapshot
                JOIN dbo.Orders AS o ON o.OrderId=snapshot.OrderId
                JOIN {usersDatabase}.dbo.AspNetUsers AS customer ON customer.Id=o.UserId
                JOIN {usersDatabase}.dbo.AspNetUsers AS mechanic ON mechanic.Id=o.MechanicId
                JOIN {vehiclesDatabase}.dbo.Vehicles AS v ON v.Id=o.VehicleId AND v.UserId=o.UserId
                WHERE o.Status=N'Completed' AND o.PlannedCompletionAt IS NOT NULL AND o.WorkCompletedAt>o.WorkStartedAt
                    AND EXISTS (SELECT 1 FROM dbo.OrderItems AS i WHERE i.OrderId=o.OrderId);
                """);
            var items = await Scalar<int>($"""
                SELECT COUNT(*) FROM dbo.MlSeedVehicleSnapshots AS snapshot
                JOIN dbo.OrderItems AS i ON i.OrderId=snapshot.OrderId
                JOIN {catalogDatabase}.dbo.Services AS s ON s.ServiceId=i.ProductId
                JOIN {catalogDatabase}.dbo.Categories AS c ON c.CategoryId=s.CategoryId;
                """);
            if (orders != data.Repairs.Length || items != data.Repairs.Sum(x => x.Services.Length))
                throw new InvalidOperationException("Seed integrity check failed: order counts or related records are inconsistent.");
        }

        var existing = await Scalar<string?>("SELECT Configuration FROM dbo.MlSeedBatch WHERE Id=1;");
        if (existing is not null)
        {
            if (existing != configuration)
                throw new InvalidOperationException("This test database already contains a different seed batch. Use a fresh isolated volume for another dataset.");
            await VerifyStoredBatch();
            await transaction.CommitAsync();
            return false;
        }

        var categories = new Dictionary<string, int>();
        foreach (var category in SeedData.Catalog.Select(x => x.Category).Distinct())
            categories[category] = await Scalar<int>($"""
                DECLARE @Id int = (SELECT CategoryId FROM {catalogDatabase}.dbo.Categories WHERE Name=@Name);
                IF @Id IS NULL
                BEGIN
                    INSERT INTO {catalogDatabase}.dbo.Categories (Name) VALUES (@Name);
                    SET @Id = CONVERT(int,SCOPE_IDENTITY());
                END;
                SELECT @Id;
                """, new { Name = category });

        var supplierIds = new List<int>();
        foreach (var name in Suppliers)
            supplierIds.Add(await Scalar<int>($"""
                DECLARE @Id int = (SELECT TOP (1) SupplierId FROM {catalogDatabase}.dbo.Suppliers WHERE Name=@Name ORDER BY SupplierId);
                IF @Id IS NULL
                BEGIN
                    INSERT INTO {catalogDatabase}.dbo.Suppliers (Name, Phone) VALUES (@Name, @Phone);
                    SET @Id = CONVERT(int,SCOPE_IDENTITY());
                END;
                SELECT @Id;
                """, new { Name = name, Phone = $"+380000000{supplierIds.Count:D3}" }));

        var serviceIds = new Dictionary<ServiceSpec, int>();
        foreach (var spec in SeedData.Catalog)
        {
            var supplierIndex = serviceIds.Count % supplierIds.Count;
            serviceIds[spec] = await Scalar<int>($"""
                DECLARE @Id int = (SELECT TOP (1) ServiceId FROM {catalogDatabase}.dbo.Services
                    WHERE Name=@Name AND CategoryId=@CategoryId ORDER BY ServiceId);
                IF @Id IS NULL
                BEGIN
                    INSERT INTO {catalogDatabase}.dbo.Services (Name, Price, CategoryId) VALUES (@Name, @Price, @CategoryId);
                    SET @Id = CONVERT(int,SCOPE_IDENTITY());
                END;
                IF NOT EXISTS (SELECT 1 FROM {catalogDatabase}.dbo.ServiceDetails WHERE ServiceId=@Id)
                    INSERT INTO {catalogDatabase}.dbo.ServiceDetails (ServiceId, Manufacturer, Warranty)
                    VALUES (@Id, @Manufacturer, N'3 місяці');
                IF NOT EXISTS (SELECT 1 FROM {catalogDatabase}.dbo.ServiceSupplier WHERE ServiceId=@Id AND SupplierId=@SupplierId)
                    INSERT INTO {catalogDatabase}.dbo.ServiceSupplier (ServiceId, SupplierId) VALUES (@Id, @SupplierId);
                SELECT @Id;
                """, new { spec.Name, spec.Price, CategoryId = categories[spec.Category],
                    Manufacturer = Suppliers[supplierIndex], SupplierId = supplierIds[supplierIndex] });
        }

        var roles = new Dictionary<string, string>();
        foreach (var role in new[] { "User", "Mechanic" })
            roles[role] = await Scalar<string>($"""
                DECLARE @Id nvarchar(450) = (SELECT Id FROM {usersDatabase}.dbo.AspNetRoles WHERE NormalizedName=@Normalized);
                IF @Id IS NULL
                BEGIN
                    SET @Id=@NewId;
                    INSERT INTO {usersDatabase}.dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES (@Id, @Name, @Normalized, @NewId);
                END;
                SELECT @Id;
                """, new { Name = role, Normalized = role.ToUpperInvariant(), NewId = $"ml-seed-role-{role}" });

        var hasher = new PasswordHasher<ApplicationUser>();
        foreach (var person in data.Customers.Concat(data.Mechanics))
        {
            var email = $"{person.UserName}@autoservice.test";
            var identity = new ApplicationUser { Id = person.Id, UserName = person.UserName };
            var passwordHash = string.IsNullOrWhiteSpace(demoPassword) ? null : hasher.HashPassword(identity, demoPassword);
            await Execute($"""
                INSERT INTO {usersDatabase}.dbo.AspNetUsers
                    (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, FullName,
                     PasswordHash, ConcurrencyStamp, SecurityStamp, AccessFailedCount, LockoutEnabled, PhoneNumberConfirmed, TwoFactorEnabled)
                VALUES (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, 1, @FullName,
                        @PasswordHash, NEWID(), NEWID(), 0, 1, 0, 0);
                INSERT INTO {usersDatabase}.dbo.AspNetUserRoles (UserId, RoleId) VALUES (@Id, @RoleId);
                """, new { person.Id, person.UserName, NormalizedUserName = person.UserName.ToUpperInvariant(),
                    Email = email, NormalizedEmail = email.ToUpperInvariant(), FullName = person.Name,
                    PasswordHash = passwordHash, RoleId = roles[person.Role] });
        }

        var latestMileage = data.Repairs.GroupBy(x => x.Vehicle.Id).ToDictionary(group => group.Key, group => group.Max(x => x.MileageKm));
        var created = data.Repairs[0].StartedAt.AddDays(-1);
        foreach (var car in data.Vehicles)
            await Execute($"""
                INSERT INTO {vehiclesDatabase}.dbo.Vehicles
                    (Id, UserId, Vin, Make, Model, Year, MileageKm, Color, CreatedAt, UpdatedAt)
                VALUES (@Id, @OwnerId, @Vin, @Make, @Model, @Year, @Mileage, @Color, @Created, @Updated);
                """, new { car.Id, car.OwnerId, car.Vin, car.Make, car.Model, car.Year, car.Color,
                    Mileage = latestMileage.GetValueOrDefault(car.Id, car.BaseMileage), Created = created,
                    Updated = data.Repairs.Max(x => x.CompletedAt) });

        await Execute("""
            IF OBJECT_ID(N'dbo.MlSeedVehicleSnapshots', N'U') IS NULL
                CREATE TABLE dbo.MlSeedVehicleSnapshots (
                    OrderId int NOT NULL PRIMARY KEY REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
                    VehicleYear int NOT NULL, MileageKm int NOT NULL
                );
            """);
        foreach (var repair in data.Repairs)
        {
            var car = repair.Vehicle;
            var id = await Scalar<int>("""
                INSERT INTO dbo.Orders
                    (OrderDate, Status, UserId, VehicleId, VehicleDisplayName, VehicleVin, MechanicId, MechanicName,
                     PlannedCompletionAt, WorkStartedAt, WorkCompletedAt)
                OUTPUT INSERTED.OrderId
                VALUES (@Slot, N'Completed', @OwnerId, @VehicleId, @DisplayName, @Vin, @MechanicId, @MechanicName,
                        @PlannedAt, @StartedAt, @CompletedAt);
                """, new { repair.Slot, car.OwnerId, VehicleId = car.Id,
                    DisplayName = FormattableString.Invariant($"{car.Make} {car.Model} ({car.Year})"), car.Vin,
                    MechanicId = repair.Mechanic.Id, MechanicName = repair.Mechanic.Name,
                    repair.PlannedAt, repair.StartedAt, repair.CompletedAt });
            await Execute("INSERT INTO dbo.MlSeedVehicleSnapshots (OrderId, VehicleYear, MileageKm) VALUES (@Id,@Year,@MileageKm);",
                new { Id = id, car.Year, repair.MileageKm });
            foreach (var service in repair.Services)
                await Execute("INSERT INTO dbo.OrderItems (OrderId, ProductId, Quantity) VALUES (@Id,@ServiceId,1);",
                    new { Id = id, ServiceId = serviceIds[service] });
        }

        await VerifyStoredBatch();
        await Execute("INSERT INTO dbo.MlSeedBatch (Id, Configuration) VALUES (1,@Configuration);", new { Configuration = configuration });
        await transaction.CommitAsync();
        return true;
    }
}
