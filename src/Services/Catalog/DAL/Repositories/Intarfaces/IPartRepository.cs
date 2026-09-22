using AutoServiceCatalog.DAL.Entities;
using AutoServiceCatalog.DAL.QueryParametrs;
using AutoServiceCatalog.DAL.Specefication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Repositories.Intarfaces
{
    public interface IServiceRepository : IGenericRepository<Service>
    {
        Task<List<Service>> GetServicesAbovePriceAsync(decimal price);
        Task<List<Service>> GetServicesBelowPriceAsync(decimal price);
        Task<List<Service>> SearchByNameAsync(string keyword);

        Task<PagedResult<Service>> GetServicesAsync(PartQueryParameters parameters);
    }
}
