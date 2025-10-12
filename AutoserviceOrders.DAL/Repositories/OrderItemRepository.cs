using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.DAL.Repositories.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.DAL.Repositories
{
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(IDbConnection connection, IDbTransaction transaction = null)
            : base(connection, "OrderItems", "OrderItemId", transaction)
        {
        }

        public async Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(int orderId)
        {
            string sql = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";
            return await _connection.QueryAsync<OrderItem>(sql, new { OrderId = orderId }, transaction: _transaction);
        }
    }
}
