using AutoServiceCatalog.BLL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Services.Interfaces
{
    public interface IServiceDetailService
    {
        Task<List<ServiceDetailDto>> GetAllAsync();
        Task<ServiceDetailDto?> GetByIdAsync(int id);
        Task<ServiceDetailDto> CreateAsync(ServiceDetailCreateDto dto);
        Task UpdateAsync(int id, ServiceDetailCreateDto dto);
        Task DeleteAsync(int id);
        Task<List<ServiceDetailDto>> GetByManufacturerAsync(string manufacturer);
    }
}
