using AutoServiceCatalog.DAL.Repositories.Intarfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.UOW
{
    public interface IUnitOfWork : IDisposable
    {
        IServiceRepository Services { get; }
        ICategoryRepository Categories { get; }
        ISupplierRepository Suppliers { get; }
        IServiceDetailRepository ServiceDetail { get; }
        IServiceSupplierRepository ServiceSupplier { get; }

        Task<int> SaveChangesAsync();
    }
}
