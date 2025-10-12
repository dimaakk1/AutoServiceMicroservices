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
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(IDbConnection connection, IDbTransaction transaction = null)
            : base(connection, "Products", "ProductId", transaction)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsByNameAsync(string name)
        {
            string sql = "SELECT * FROM Products WHERE Name LIKE @Name";
            return await _connection.QueryAsync<Product>(sql, new { Name = "%" + name + "%" }, transaction: _transaction);
        }
    }
}
