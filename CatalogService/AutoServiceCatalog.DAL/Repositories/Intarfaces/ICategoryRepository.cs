using AutoServiceCatalog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Repositories.Intarfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<List<Service>> GetServicesByCategoryNameAsync(string categoryName);
    }
}
