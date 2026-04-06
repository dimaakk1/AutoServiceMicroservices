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
            using (var masterConnection = new SqlConnection(connectionString))
            {
                masterConnection.Open();

                var createDbCommand = @"
            IF DB_ID('OrdersDb') IS NULL
            BEGIN
                CREATE DATABASE OrdersDb;
            END";

                using var cmd = new SqlCommand(createDbCommand, masterConnection);
                cmd.ExecuteNonQuery();
            }

            // Підключення до нової бази для створення таблиць
            using var conn = new SqlConnection(connectionString);
            conn.Open();


            var createTablesScript = @"
        IF OBJECT_ID('Customers') IS NULL
        CREATE TABLE Customers (
            CustomerId INT IDENTITY(1,1) PRIMARY KEY,
            FullName NVARCHAR(100) NOT NULL,
            Phone NVARCHAR(20),
            Email NVARCHAR(100)
        );

        IF OBJECT_ID('Orders') IS NULL
        CREATE TABLE Orders (
            OrderId INT IDENTITY(1,1) PRIMARY KEY,
            CustomerId INT NOT NULL,
            OrderDate DATETIME DEFAULT GETDATE(),
            Status NVARCHAR(50),
            FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
        );

        IF OBJECT_ID('OrderDetails') IS NULL
        CREATE TABLE OrderDetails (
            OrderId INT PRIMARY KEY,
            MechanicName NVARCHAR(100),
            EstimatedCompletionDate DATETIME,
            FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
        );

        IF OBJECT_ID('Products') IS NULL

        IF OBJECT_ID('OrderItems') IS NULL
        CREATE TABLE OrderItems (
            OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
            OrderId INT NOT NULL,
            ProductId INT NOT NULL,
            Quantity INT NOT NULL,
            FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
        );";

            using var cmd2 = new SqlCommand(createTablesScript, conn);
            cmd2.ExecuteNonQuery();
        }
    }
}
