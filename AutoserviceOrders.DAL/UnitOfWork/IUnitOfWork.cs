using AutoserviceOrders.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.DAL.UnitOfWork
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        ICustomerRepository Customers { get; }
        IOrderRepository Orders { get; }
        IProductRepository Products { get; }
        IOrderDetailsRepository OrderDetails { get; }
        IOrderItemRepository OrderItems { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
