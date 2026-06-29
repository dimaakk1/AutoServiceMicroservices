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
        IOrderRepository? Orders { get; }
        IOrderDetailsRepository? OrderDetails { get; }
        IOrderItemRepository? OrderItems { get; }
        IPaymentRepository? Payments { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        T GetRepository<T>() where T : class;
    }
}
