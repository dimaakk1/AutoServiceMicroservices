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
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(IDbConnection connection, IDbTransaction transaction = null)
            : base(connection, "Customers", "CustomerId", transaction)
        {
        }

        public async Task<Customer> GetByEmailAsync(string email)
        {
            string sql = "SELECT * FROM Customers WHERE Email = @Email";
            return await _connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Email = email }, transaction: _transaction);
        }
    }
}
