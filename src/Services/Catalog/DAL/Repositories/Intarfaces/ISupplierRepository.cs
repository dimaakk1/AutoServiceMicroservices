using AutoServiceCatalog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Repositories.Intarfaces
{
    public interface ISupplierRepository : IGenericRepository<Supplier>
    {
        Task<Supplier?> GetSupplierWithServicesAsync(int id);
        Task<List<Supplier>> SearchByNameAsync(string keyword);
    }
}
