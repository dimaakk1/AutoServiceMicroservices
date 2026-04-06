using AutoServiceCatalog.DAL.Db;
using AutoServiceCatalog.DAL.Entities;
using AutoServiceCatalog.DAL.QueryParametrs;
using AutoServiceCatalog.DAL.Repositories.Intarfaces;
using AutoServiceCatalog.DAL.Specefication;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Repositories
{
    public class ServiceRepository : GenericRepository<Service>, IServiceRepository
    {
        public ServiceRepository(CarServiceContext context) : base(context) { }

        public async Task<PagedResult<Service>> GetServicesAsync(PartQueryParameters parameters)
        {
            var spec = new ServiceSpecification(parameters);

            var query = SpecificationEvaluator.GetQuery<Service>(_context.Services.AsQueryable(), spec);

            var totalCount = await _context.Services
                .Where(spec.Criteria)
                .CountAsync();

            var items = await query.ToListAsync();

            return new PagedResult<Service>(items, totalCount, parameters.PageSize);
        }

        public async Task<List<Service>> GetServicesAbovePriceAsync(decimal price)
        {
            return await _context.Services
                .Where(s => s.Price > price)
                .ToListAsync();
        }

        public async Task<List<Service>> GetServicesBelowPriceAsync(decimal price)
        {
            return await _context.Services
                .Where(s => s.Price < price)
                .ToListAsync();
        }

        public async Task<List<Service>> SearchByNameAsync(string keyword)
        {
            return await _context.Services
                .Where(s => s.Name.Contains(keyword))
                .ToListAsync();
        }
    }
}
