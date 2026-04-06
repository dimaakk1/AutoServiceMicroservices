using AutoServiceCatalog.BLL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Services.Interfaces
{
    public interface IServiceSupplierService
    {
        Task<List<ServiceSupplierDto>> GetAllAsync();
        Task<ServiceSupplierDto?> GetByIdsAsync(int serviceId, int supplierId);
        Task<ServiceSupplierDto> CreateAsync(ServiceSupplierDto dto);
        Task DeleteAsync(int serviceId, int supplierId);
        Task<List<SupplierDto>> GetSuppliersByServiceIdAsync(int serviceId);
        Task<List<ServiceDto>> GetServicesBySupplierIdAsync(int supplierId);
    }
}
