using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.DAL.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.DAL.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction? _transaction;

        public PaymentRepository(IDbConnection connection, IDbTransaction? transaction = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _transaction = transaction;
        }

        private SqlConnection SqlConn => (SqlConnection)_connection;
        private SqlTransaction? SqlTrans => (SqlTransaction?)_transaction;

        public async Task<int> AddAsync(Payment payment)
        {
            const string sql = @"
INSERT INTO Payments (OrderId, Amount, Currency, Status, PaymentMethod, TransactionId, CreatedAt, PaidAt, Description)
VALUES (@OrderId, @Amount, @Currency, @Status, @PaymentMethod, @TransactionId, @CreatedAt, @PaidAt, @Description);
SELECT SCOPE_IDENTITY();";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);

            cmd.Parameters.AddWithValue("@OrderId", payment.OrderId);
            cmd.Parameters.AddWithValue("@Amount", payment.Amount);
            cmd.Parameters.AddWithValue("@Currency", payment.Currency);
            cmd.Parameters.AddWithValue("@Status", payment.Status);
            cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
            cmd.Parameters.AddWithValue("@TransactionId", payment.TransactionId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedAt", payment.CreatedAt);
            cmd.Parameters.AddWithValue("@PaidAt", payment.PaidAt ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", payment.Description ?? (object)DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<Payment?> GetByIdAsync(int paymentId)
        {
            const string sql = "SELECT * FROM Payments WHERE PaymentId = @PaymentId";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@PaymentId", paymentId);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapPayment(reader);
            }

            return null;
        }

        public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
        {
            const string sql = "SELECT * FROM Payments WHERE OrderId = @OrderId ORDER BY CreatedAt DESC";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@OrderId", orderId);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapPayment(reader);
            }

            return null;
        }

        public async Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId)
        {
            const string sql = "SELECT * FROM Payments WHERE TransactionId = @TransactionId";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@TransactionId", transactionId);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapPayment(reader);
            }

            return null;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(string userId)
        {
            const string sql = @"
SELECT p.* FROM Payments p
INNER JOIN Orders o ON p.OrderId = o.OrderId
WHERE o.UserId = @UserId
ORDER BY p.CreatedAt DESC";

            var payments = new List<Payment>();

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                payments.Add(MapPayment(reader));
            }

            return payments;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Payments ORDER BY CreatedAt DESC";
            var payments = new List<Payment>();

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                payments.Add(MapPayment(reader));
            }

            return payments;
        }

        public async Task<int> UpdateAsync(Payment payment)
        {
            const string sql = @"
UPDATE Payments 
SET OrderId = @OrderId, Amount = @Amount, Currency = @Currency, 
    Status = @Status, PaymentMethod = @PaymentMethod, 
    TransactionId = @TransactionId, PaidAt = @PaidAt, Description = @Description
WHERE PaymentId = @PaymentId";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);

            cmd.Parameters.AddWithValue("@PaymentId", payment.PaymentId);
            cmd.Parameters.AddWithValue("@OrderId", payment.OrderId);
            cmd.Parameters.AddWithValue("@Amount", payment.Amount);
            cmd.Parameters.AddWithValue("@Currency", payment.Currency);
            cmd.Parameters.AddWithValue("@Status", payment.Status);
            cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
            cmd.Parameters.AddWithValue("@TransactionId", payment.TransactionId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PaidAt", payment.PaidAt ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", payment.Description ?? (object)DBNull.Value);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DeleteAsync(int paymentId)
        {
            const string sql = "DELETE FROM Payments WHERE PaymentId = @PaymentId";

            await using var cmd = new SqlCommand(sql, SqlConn, SqlTrans);
            cmd.Parameters.AddWithValue("@PaymentId", paymentId);

            return await cmd.ExecuteNonQueryAsync();
        }

        private Payment MapPayment(SqlDataReader reader)
        {
            return new Payment
            {
                PaymentId = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                Currency = reader["Currency"]?.ToString() ?? "UAH",
                Status = reader["Status"]?.ToString() ?? "Pending",
                PaymentMethod = reader["PaymentMethod"]?.ToString() ?? "LiqPay",
                TransactionId = reader["TransactionId"]?.ToString(),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                PaidAt = reader["PaidAt"] == DBNull.Value ? null : reader.GetDateTime(reader.GetOrdinal("PaidAt")),
                Description = reader["Description"]?.ToString()
            };
        }
    }
}
