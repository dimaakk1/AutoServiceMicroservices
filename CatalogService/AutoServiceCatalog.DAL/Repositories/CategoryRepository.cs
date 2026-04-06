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
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(CarServiceContext context) : base(context) { }

        public async Task<List<Service>> GetServicesByCategoryNameAsync(string categoryName)
        {
            return await _context.Categories
                .Where(c => c.Name == categoryName)
                .SelectMany(c => c.Services)
                .ToListAsync();
        }
    }
}
