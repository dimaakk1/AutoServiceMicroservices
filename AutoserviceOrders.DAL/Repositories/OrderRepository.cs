using AutoserviceOrders.DAL.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using AutoserviceOrders.DAL.Repositories.Interfaces;

namespace AutoserviceOrders.DAL.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction? _transaction;

        public OrderRepository(IDbConnection connection, IDbTransaction? transaction = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _transaction = transaction;
        }

        private SqlConnection SqlConn => (SqlConnection)_connection;
        private SqlTransaction? SqlTrans => (SqlTransaction?)_transaction;

        public async Task<int> AddAsync(Order order)
        {
            const string sql = @"
                INSERT INTO Orders (CustomerId, OrderDate, Status)
                VALUES (@CustomerId, @OrderDate, @Status);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@CustomerId", order.CustomerId);
            cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
            cmd.Parameters.AddWithValue("@Status", order.Status);

            object? result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<Order?> GetByIdAsync(int orderId)
        {
            const string sql = "SELECT * FROM Orders WHERE OrderId = @OrderId";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@OrderId", orderId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Order
                {
                    OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                    OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                    Status = reader["Status"]?.ToString() ?? string.Empty
                };
            }

            return null;
        }

        public async Task<List<Order>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Orders";
            var orders = new List<Order>();

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                orders.Add(new Order
                {
                    OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                    OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                    Status = reader["Status"]?.ToString() ?? string.Empty
                });
            }

            return orders;
        }

        public async Task<int> UpdateAsync(Order order)
        {
            const string sql = @"
                UPDATE Orders
                SET CustomerId = @CustomerId,
                    OrderDate = @OrderDate,
                    Status = @Status
                WHERE OrderId = @OrderId";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@CustomerId", order.CustomerId);
            cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
            cmd.Parameters.AddWithValue("@Status", order.Status);
            cmd.Parameters.AddWithValue("@OrderId", order.OrderId);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DeleteAsync(int orderId)
        {
            const string sql = "DELETE FROM Orders WHERE OrderId = @OrderId";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@OrderId", orderId);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Order>> GetOrdersByCustomerAsync(int customerId)
        {
            const string sql = "SELECT * FROM Orders WHERE CustomerId = @CustomerId";
            var orders = new List<Order>();

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@CustomerId", customerId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                orders.Add(new Order
                {
                    OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                    OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                    Status = reader["Status"]?.ToString() ?? string.Empty
                });
            }

            return orders;
        }
    }
}
