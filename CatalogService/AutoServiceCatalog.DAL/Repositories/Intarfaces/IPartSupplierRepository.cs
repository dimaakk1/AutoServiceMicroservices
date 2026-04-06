using AutoServiceCatalog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Repositories.Intarfaces
{
    public interface IServiceSupplierRepository : IGenericRepository<ServiceSupplier>
    {
        Task<List<Supplier>> GetSuppliersByServiceIdAsync(int serviceId);
        Task<List<Service>> GetServicesBySupplierIdAsync(int supplierId);
    }
}
