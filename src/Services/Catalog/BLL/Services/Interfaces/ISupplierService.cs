using AutoServiceCatalog.BLL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<List<SupplierDto>> GetAllAsync();
        Task<SupplierDto> GetByIdAsync(int id);
        Task<SupplierDto> CreateAsync(SupplierCreateDto dto);
        Task UpdateAsync(int id, SupplierCreateDto dto);
        Task DeleteAsync(int id);
        Task<SupplierDto> GetSupplierWithServicesAsync(int id);
        Task<List<SupplierDto>> SearchByNameAsync(string keyword);
    }
}
