using AutoServiceCatalog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Repositories.Intarfaces
{
    public interface IServiceDetailRepository : IGenericRepository<ServiceDetail>
    {
        Task<List<ServiceDetail>> GetByManufacturerAsync(string manufacturer);
    }
}
