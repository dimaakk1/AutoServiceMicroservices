using AutoServiceCatalog.BLL.DTO;
using AutoServiceCatalog.DAL.Entities;
using AutoServiceCatalog.DAL.QueryParametrs;
using AutoServiceCatalog.DAL.Specefication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Services.Interfaces
{
    public interface IServiceService
    {
        Task<List<ServiceDto>> GetAllAsync();
        Task<ServiceDto> GetByIdAsync(int id);
        Task<ServiceDto> CreateAsync(ServiceCreateDto dto);
        Task UpdateAsync(int id, ServiceCreateDto dto);
        Task DeleteAsync(int id);
        Task<List<ServiceDto>> SearchByNameAsync(string keyword);
        Task<List<ServiceDto>> GetServicesAbovePriceAsync(decimal price);
        Task<List<ServiceDto>> GetServicesBelowPriceAsync(decimal price);
        Task<PagedResult<Service>> GetServicesAsync(PartQueryParameters parameters);
    }
}
