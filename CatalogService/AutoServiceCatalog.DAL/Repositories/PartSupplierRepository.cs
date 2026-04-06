using AutoServiceCatalog.DAL.Db;
using AutoServiceCatalog.DAL.Entities;
using AutoServiceCatalog.DAL.Repositories.Intarfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Repositories
{
    public class ServiceSupplierRepository : GenericRepository<ServiceSupplier>, IServiceSupplierRepository
    {
        public ServiceSupplierRepository(CarServiceContext context) : base(context) { }
        public async Task<List<Supplier>> GetSuppliersByServiceIdAsync(int serviceId)
        {
            return await _context.ServiceSupplier
                .Where(ss => ss.ServiceId == serviceId)
                .Select(ss => ss.Supplier)
                .ToListAsync();
        }
        public async Task<List<Service>> GetServicesBySupplierIdAsync(int supplierId)
        {
            return await _context.ServiceSupplier
                .Where(ss => ss.SupplierId == supplierId)
                .Select(ss => ss.Service)
                .ToListAsync();
        }
    }
}
