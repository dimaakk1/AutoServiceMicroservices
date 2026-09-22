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
    public class ServiceDetailRepository : GenericRepository<ServiceDetail>, IServiceDetailRepository
    {
        public ServiceDetailRepository(CarServiceContext context) : base(context) { }
        public async Task<List<ServiceDetail>> GetByManufacturerAsync(string manufacturer)
        {
            return await _context.ServiceDetails
                .Where(sd => sd.Manufacturer.ToLower().Contains(manufacturer.ToLower()))
                .Include(sd => sd.Service)
                .ToListAsync();
        }
    }
}
