using AutoserviceOrders.DAL.Models;
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
INSERT INTO Orders (OrderDate, Status, UserId, VehicleId, VehicleDisplayName, VehicleVin, VehicleLicensePlate)
VALUES (@OrderDate, @Status, @UserId, @VehicleId, @VehicleDisplayName, @VehicleVin, @VehicleLicensePlate);
SELECT SCOPE_IDENTITY();";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);

            cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
            cmd.Parameters.AddWithValue("@Status", order.Status);
            cmd.Parameters.AddWithValue("@UserId", order.UserId);
            cmd.Parameters.AddWithValue("@VehicleId", (object?)order.VehicleId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VehicleDisplayName", (object?)order.VehicleDisplayName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VehicleVin", (object?)order.VehicleVin ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VehicleLicensePlate", (object?)order.VehicleLicensePlate ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<Order?> GetByIdAsync(int orderId)
        {
            const string sql = "SELECT * FROM Orders WHERE OrderId = @OrderId";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@OrderId", orderId);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return ReadOrder(reader);
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
                orders.Add(ReadOrder(reader));
            }

            return orders;
        }

        public async Task<int> UpdateAsync(Order order)
        {
            const string sql = @"
                UPDATE Orders
                SET OrderDate = @OrderDate,
                    Status = @Status,
                    WorkStartedAt = CASE
                        WHEN @Status IN ('Diagnosing', 'InProgress') AND WorkCompletedAt IS NULL
                            THEN COALESCE(WorkStartedAt, SYSUTCDATETIME())
                        ELSE WorkStartedAt END,
                    WorkCompletedAt = CASE WHEN @Status = 'Completed'
                        THEN COALESCE(WorkCompletedAt, SYSUTCDATETIME())
                        ELSE WorkCompletedAt END
                WHERE OrderId = @OrderId";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
            cmd.Parameters.AddWithValue("@Status", order.Status);
            cmd.Parameters.AddWithValue("@OrderId", order.OrderId);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Order>> GetFilteredAsync(OrderReadFilter filter)
        {
            var sql = @"SELECT * FROM Orders
                WHERE (@UserId IS NULL OR UserId = @UserId)
                  AND (@MechanicId IS NULL OR MechanicId = @MechanicId)
                  AND (@Status IS NULL OR Status = @Status)
                  AND (@Day IS NULL OR (OrderDate >= @Day AND OrderDate < DATEADD(day, 1, @Day)))
                  AND (@FromDate IS NULL OR OrderDate >= @FromDate)
                  AND (@ToDate IS NULL OR OrderDate <= @ToDate)
                ORDER BY OrderDate DESC, OrderId DESC";
            if (filter.Limit.HasValue)
                sql += " OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 450).Value = (object?)filter.UserId ?? DBNull.Value;
            cmd.Parameters.Add("@MechanicId", SqlDbType.NVarChar, 450).Value = (object?)filter.MechanicId ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = (object?)filter.Status ?? DBNull.Value;
            cmd.Parameters.Add("@Day", SqlDbType.DateTime2).Value = (object?)filter.Date?.Date ?? DBNull.Value;
            cmd.Parameters.Add("@FromDate", SqlDbType.DateTime2).Value = (object?)filter.FromDate ?? DBNull.Value;
            cmd.Parameters.Add("@ToDate", SqlDbType.DateTime2).Value = (object?)filter.ToDate ?? DBNull.Value;
            if (filter.Limit.HasValue)
            {
                cmd.Parameters.Add("@Offset", SqlDbType.Int).Value = filter.Offset;
                cmd.Parameters.Add("@Limit", SqlDbType.Int).Value = filter.Limit.Value;
            }
            var orders = new List<Order>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                orders.Add(ReadOrder(reader));
            return orders;
        }

        public async Task<int> DeleteAsync(int orderId)
        {
            const string sql = "DELETE FROM Orders WHERE OrderId = @OrderId";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@OrderId", orderId);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Order>> GetByUserIdAsync(string userId)
        {
            const string sql = "SELECT * FROM Orders WHERE UserId = @UserId";

            var orders = new List<Order>();

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                orders.Add(ReadOrder(reader));
            }

            return orders;
        }

        public async Task<List<Order>> GetByMechanicIdAsync(string mechanicId)
        {
            const string sql = "SELECT * FROM Orders WHERE MechanicId = @MechanicId ORDER BY OrderDate";
            var orders = new List<Order>();

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@MechanicId", mechanicId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                orders.Add(ReadOrder(reader));

            return orders;
        }

        public async Task<int> AssignMechanicAsync(int orderId, string mechanicId, string mechanicName)
        {
            const string sql = @"
                UPDATE Orders
                SET MechanicId = @MechanicId,
                    MechanicName = @MechanicName
                WHERE OrderId = @OrderId";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@MechanicId", mechanicId);
            cmd.Parameters.AddWithValue("@MechanicName", mechanicName);
            cmd.Parameters.AddWithValue("@OrderId", orderId);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> UpdateStatusAsync(
            int orderId,
            string status,
            DateTime? workStartedAt,
            DateTime? workCompletedAt)
        {
            const string sql = @"
                UPDATE Orders
                SET Status = @Status,
                    WorkStartedAt = CASE WHEN WorkCompletedAt IS NULL
                        THEN COALESCE(WorkStartedAt, @WorkStartedAt) ELSE WorkStartedAt END,
                    WorkCompletedAt = COALESCE(WorkCompletedAt, @WorkCompletedAt)
                WHERE OrderId = @OrderId";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@WorkStartedAt", (object?)workStartedAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WorkCompletedAt", (object?)workCompletedAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OrderId", orderId);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> UpdateMechanicStatusAsync(
            int orderId,
            string mechanicId,
            string status,
            DateTime? workStartedAt,
            DateTime? workCompletedAt)
        {
            const string sql = @"
                UPDATE Orders
                SET Status = @Status,
                    WorkStartedAt = CASE WHEN WorkCompletedAt IS NULL
                        THEN COALESCE(WorkStartedAt, @WorkStartedAt) ELSE WorkStartedAt END,
                    WorkCompletedAt = COALESCE(WorkCompletedAt, @WorkCompletedAt)
                WHERE OrderId = @OrderId AND MechanicId = @MechanicId";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@WorkStartedAt", (object?)workStartedAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WorkCompletedAt", (object?)workCompletedAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OrderId", orderId);
            cmd.Parameters.AddWithValue("@MechanicId", mechanicId);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> SetPlannedCompletionAsync(int orderId, DateTime plannedCompletionUtc, DateTime plannedCompletionShopTime)
        {
            const string sql = @"
                UPDATE Orders
                SET PlannedCompletionAt = @PlannedCompletionAt
                WHERE OrderId = @OrderId
                  AND Status IN ('Pending', 'Confirmed')
                  AND WorkStartedAt IS NULL AND WorkCompletedAt IS NULL
                  AND @PlannedCompletionShopTime > OrderDate;";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
            cmd.Parameters.Add("@PlannedCompletionAt", SqlDbType.DateTime2).Value = plannedCompletionUtc;
            cmd.Parameters.Add("@PlannedCompletionShopTime", SqlDbType.DateTime2).Value = plannedCompletionShopTime;
            return await cmd.ExecuteNonQueryAsync();
        }

        private static DateTime? ReadUtc(SqlDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? null : DateTime.SpecifyKind(reader.GetDateTime(ordinal), DateTimeKind.Utc);
        }

        private static Order ReadOrder(SqlDataReader reader)
        {
            var vehicleIdOrdinal = reader.GetOrdinal("VehicleId");
            return new Order
            {
                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                Status = reader["Status"]?.ToString() ?? string.Empty,
                UserId = reader["UserId"]?.ToString() ?? string.Empty,
                VehicleId = reader.IsDBNull(vehicleIdOrdinal) ? null : reader.GetGuid(vehicleIdOrdinal),
                VehicleDisplayName = reader["VehicleDisplayName"] as string,
                VehicleVin = reader["VehicleVin"] as string,
                VehicleLicensePlate = reader["VehicleLicensePlate"] as string,
                MechanicId = reader["MechanicId"] as string,
                MechanicName = reader["MechanicName"] as string,
                WorkStartedAt = ReadUtc(reader, "WorkStartedAt"),
                WorkCompletedAt = ReadUtc(reader, "WorkCompletedAt"),
                PlannedCompletionAt = ReadUtc(reader, "PlannedCompletionAt")
            };
        }
    }
}
