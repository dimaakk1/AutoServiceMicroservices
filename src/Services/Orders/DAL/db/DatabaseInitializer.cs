using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.DAL.db
{
    public static class DatabaseInitializer
    {
        public static void Initialize(string connectionString)
        {
            // Підключення до master для створення бази, якщо її ще нема
            var databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
            if (string.IsNullOrWhiteSpace(databaseName) || databaseName.Length > 128 ||
                new[] { "master", "tempdb", "model", "msdb" }.Contains(databaseName, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Specify a valid Orders database in the connection string.", nameof(connectionString));

            var masterConnectionString = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            }.ConnectionString;

            using (var masterConnection = new SqlConnection(masterConnectionString))
            {
                masterConnection.Open();

                var createDbCommand = @"
            IF DB_ID(@DatabaseName) IS NULL
            BEGIN
                DECLARE @CreateSql nvarchar(300) = N'CREATE DATABASE ' + QUOTENAME(@DatabaseName);
                EXEC (@CreateSql);
            END";

                using var cmd = new SqlCommand(createDbCommand, masterConnection);
                cmd.Parameters.Add("@DatabaseName", System.Data.SqlDbType.NVarChar, 128).Value = databaseName;
                cmd.ExecuteNonQuery();
            }

            // Підключення до нової бази для створення таблиць
            using var conn = new SqlConnection(connectionString);
            conn.Open();


            var createTablesScript = @"

        IF OBJECT_ID('Orders') IS NULL
        CREATE TABLE Orders (
            OrderId INT IDENTITY(1,1) PRIMARY KEY,
            OrderDate DATETIME DEFAULT GETDATE(),
            Status NVARCHAR(50),
            UserId NVARCHAR(450),
            VehicleId UNIQUEIDENTIFIER NULL,
            VehicleDisplayName NVARCHAR(250) NULL,
            VehicleVin NVARCHAR(17) NULL,
            VehicleLicensePlate NVARCHAR(20) NULL,
            MechanicId NVARCHAR(450) NULL,
            MechanicName NVARCHAR(200) NULL,
            WorkStartedAt DATETIME2 NULL,
            WorkCompletedAt DATETIME2 NULL,
            PlannedCompletionAt DATETIME2 NULL
        );

        IF COL_LENGTH('Orders', 'VehicleId') IS NULL
            ALTER TABLE Orders ADD VehicleId UNIQUEIDENTIFIER NULL;
        IF COL_LENGTH('Orders', 'VehicleDisplayName') IS NULL
            ALTER TABLE Orders ADD VehicleDisplayName NVARCHAR(250) NULL;
        IF COL_LENGTH('Orders', 'VehicleVin') IS NULL
            ALTER TABLE Orders ADD VehicleVin NVARCHAR(17) NULL;
        IF COL_LENGTH('Orders', 'VehicleLicensePlate') IS NULL
            ALTER TABLE Orders ADD VehicleLicensePlate NVARCHAR(20) NULL;
        IF COL_LENGTH('Orders', 'MechanicId') IS NULL
            ALTER TABLE Orders ADD MechanicId NVARCHAR(450) NULL;
        IF COL_LENGTH('Orders', 'MechanicName') IS NULL
            ALTER TABLE Orders ADD MechanicName NVARCHAR(200) NULL;
        IF COL_LENGTH('Orders', 'WorkStartedAt') IS NULL
            ALTER TABLE Orders ADD WorkStartedAt DATETIME2 NULL;
        IF COL_LENGTH('Orders', 'WorkCompletedAt') IS NULL
            ALTER TABLE Orders ADD WorkCompletedAt DATETIME2 NULL;
        IF COL_LENGTH('Orders', 'PlannedCompletionAt') IS NULL
            ALTER TABLE Orders ADD PlannedCompletionAt DATETIME2 NULL;

        IF OBJECT_ID('OrderDetails') IS NULL
        CREATE TABLE OrderDetails (
            OrderId INT PRIMARY KEY,
            MechanicName NVARCHAR(100),
            EstimatedCompletionDate DATETIME,
            FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
        );

        IF OBJECT_ID('OrderItems') IS NULL
        CREATE TABLE OrderItems (
            OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
            OrderId INT NOT NULL,
            ProductId INT NOT NULL,
            Quantity INT NOT NULL,
            FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
        );

        IF OBJECT_ID('Payments') IS NULL
        CREATE TABLE Payments (
            PaymentId INT IDENTITY(1,1) PRIMARY KEY,
            OrderId INT NOT NULL,
            Amount DECIMAL(10, 2) NOT NULL,
            Currency NVARCHAR(3) DEFAULT 'UAH',
            Status NVARCHAR(50) DEFAULT 'Pending',
            PaymentMethod NVARCHAR(50) DEFAULT 'LiqPay',
            TransactionId NVARCHAR(255),
            CreatedAt DATETIME DEFAULT GETDATE(),
            PaidAt DATETIME NULL,
            Description NVARCHAR(MAX),
            FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
        );

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'IX_Orders_OrderDate_OrderId')
            CREATE INDEX IX_Orders_OrderDate_OrderId ON dbo.Orders(OrderDate DESC, OrderId DESC);
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'IX_Orders_UserId')
            CREATE INDEX IX_Orders_UserId ON dbo.Orders(UserId);
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.OrderItems') AND name = 'IX_OrderItems_OrderId')
            CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);";

            using var cmd2 = new SqlCommand(createTablesScript, conn);
            cmd2.ExecuteNonQuery();
        }
    }
}
