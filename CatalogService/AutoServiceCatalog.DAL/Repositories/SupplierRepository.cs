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
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(CarServiceContext context) : base(context) { }
        public async Task<Supplier?> GetSupplierWithServicesAsync(int id)
        {
            return await _context.Suppliers
                .Include(s => s.ServiceSuppliers)
                .ThenInclude(ss => ss.Service)
                .FirstOrDefaultAsync(s => s.SupplierId == id);
        }
        public async Task<List<Supplier>> SearchByNameAsync(string keyword)
        {
            return await _context.Suppliers
                .Where(s => s.Name.Contains(keyword))
                .ToListAsync();
        }
    }
}
