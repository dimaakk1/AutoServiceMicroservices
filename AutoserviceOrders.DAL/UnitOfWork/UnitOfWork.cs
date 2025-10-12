using AutoserviceOrders.DAL.Repositories.Interfaces;
using AutoserviceOrders.DAL.Repositories;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;

        public ICustomerRepository Customers { get; private set; }
        public IOrderRepository Orders { get; private set; }
        public IProductRepository Products { get; private set; }
        public IOrderDetailsRepository OrderDetails { get; private set; }
        public IOrderItemRepository OrderItems { get; private set; }

        public UnitOfWork(IDbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));

            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        public async Task BeginTransactionAsync()
        {
            if (_connection.State != ConnectionState.Open)
            {
                if (_connection is SqlConnection sqlConn)
                    await sqlConn.OpenAsync();
                else
                    _connection.Open();
            }

            _transaction = _connection.BeginTransaction();

            Customers = new CustomerRepository(_connection, _transaction);
            Orders = new OrderRepository(_connection, _transaction);
            Products = new ProductRepository(_connection, _transaction);
            OrderDetails = new OrderDetailsRepository(_connection, _transaction);
            OrderItems = new OrderItemRepository(_connection, _transaction);
        }

        public Task CommitAsync()
        {
            _transaction?.Commit();
            return Task.CompletedTask;
        }

        public Task RollbackAsync()
        {
            _transaction?.Rollback();
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            _transaction?.Dispose();

            if (_connection is SqlConnection sqlConn)
                await sqlConn.DisposeAsync();
            else
                _connection.Dispose();
        }
    }
}
