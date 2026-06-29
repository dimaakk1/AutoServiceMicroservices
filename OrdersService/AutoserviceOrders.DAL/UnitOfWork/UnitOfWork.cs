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
        private IDbTransaction? _transaction;
        private readonly Dictionary<Type, object> _repositories = new();

        public IOrderRepository? Orders { get; private set; }
        public IOrderDetailsRepository? OrderDetails { get; private set; }
        public IOrderItemRepository? OrderItems { get; private set; }
        public IPaymentRepository? Payments { get; private set; }

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

            
            Orders = new OrderRepository(_connection, _transaction);
            OrderDetails = new OrderDetailsRepository(_connection, _transaction);
            OrderItems = new OrderItemRepository(_connection, _transaction);
            Payments = new PaymentRepository(_connection, _transaction);
        }

        public T GetRepository<T>() where T : class
        {
            var type = typeof(T);
            
            if (_repositories.TryGetValue(type, out var repository))
            {
                return (T)repository;
            }

            object? repoInstance = null;

            if (type == typeof(IOrderRepository))
            {
                repoInstance = Orders ?? new OrderRepository(_connection, _transaction);
            }
            else if (type == typeof(IOrderDetailsRepository))
            {
                repoInstance = OrderDetails ?? new OrderDetailsRepository(_connection, _transaction);
            }
            else if (type == typeof(IOrderItemRepository))
            {
                repoInstance = OrderItems ?? new OrderItemRepository(_connection, _transaction);
            }
            else if (type == typeof(IPaymentRepository))
            {
                repoInstance = Payments ?? new PaymentRepository(_connection, _transaction);
            }

            if (repoInstance == null)
            {
                throw new InvalidOperationException($"Repository of type {type.Name} is not registered");
            }

            _repositories[type] = repoInstance;
            return (T)repoInstance;
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
