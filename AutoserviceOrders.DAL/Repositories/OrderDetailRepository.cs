using AutoserviceOrders.DAL.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoserviceOrders.DAL.Repositories.Interfaces;

namespace AutoserviceOrders.DAL.Repositories
{
    public class OrderDetailsRepository : GenericRepository<OrderDetails>, IOrderDetailsRepository
    {
        public OrderDetailsRepository(IDbConnection connection, IDbTransaction transaction = null)
            : base(connection, "OrderDetails", "OrderId", transaction)
        {
        }

        public override async Task<int> AddAsync(OrderDetails entity)
        {
            var properties = GetProperties(entity);
            var columns = string.Join(", ", properties.Select(p => p.Name));
            var values = string.Join(", ", properties.Select(p => "@" + p.Name));

            string sql = $"INSERT INTO {_tableName} ({columns}) VALUES ({values})";

            return await _connection.ExecuteAsync(sql, entity, transaction: _transaction);
        }

        public async Task<OrderDetails> GetByOrderIdAsync(int orderId)
        {
            string sql = "SELECT * FROM OrderDetails WHERE OrderId = @OrderId";
            return await _connection.QueryFirstOrDefaultAsync<OrderDetails>(sql, new { OrderId = orderId }, transaction: _transaction);
        }
    }
}
