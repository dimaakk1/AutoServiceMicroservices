using AutoMapper;
using AutoServiceCatalog.BLL.Cache;
using AutoServiceCatalog.BLL.DTO;
using AutoServiceCatalog.BLL.Services.Interfaces;
using AutoServiceCatalog.DAL.Entities;
using AutoServiceCatalog.DAL.UOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Services
{
    public class ServiceDetailService : IServiceDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TwoLevelCacheService<List<ServiceDetailDto>> _serviceDetailCache;

        public ServiceDetailService(IUnitOfWork unitOfWork, IMapper mapper, TwoLevelCacheService<List<ServiceDetailDto>> serviceDetailCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceDetailCache = serviceDetailCache;
        }

        public async Task<List<ServiceDetailDto>> GetAllAsync()
        {
            var key = "servicedetails:all";
            return await _serviceDetailCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var entities = await _unitOfWork.ServiceDetail.GetAllAsync();
                    return _mapper.Map<List<ServiceDetailDto>>(entities);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<ServiceDetailDto>();
        }

        public async Task<ServiceDetailDto?> GetByIdAsync(int id)
        {
            var key = $"servicedetail:{id}";
            return await _serviceDetailCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var entity = await _unitOfWork.ServiceDetail.GetByIdAsync(id);
                    return entity != null ? new List<ServiceDetailDto> { _mapper.Map<ServiceDetailDto>(entity) } : new List<ServiceDetailDto>();
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<ServiceDetailDto> CreateAsync(ServiceDetailCreateDto dto)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(dto.ServiceId);
            if (service == null)
                throw new Exception("Service not found");

            var entity = _mapper.Map<ServiceDetail>(dto);
            entity.ServiceId = dto.ServiceId;

            await _unitOfWork.ServiceDetail.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _serviceDetailCache.InvalidateAsync("servicedetails:all");
            await _serviceDetailCache.InvalidateAsync($"servicedetail:{entity.ServiceDetailId}");
            await _serviceDetailCache.InvalidateAsync($"servicedetails:manufacturer:{entity.Manufacturer}");

            return _mapper.Map<ServiceDetailDto>(entity);
        }

        public async Task UpdateAsync(int id, ServiceDetailCreateDto dto)
        {
            var existing = await _unitOfWork.ServiceDetail.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("ServiceDetail not found");

            var oldManufacturer = existing.Manufacturer;

            existing.Manufacturer = dto.Manufacturer;
            existing.Warranty = dto.Warranty;

            _unitOfWork.ServiceDetail.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _serviceDetailCache.InvalidateAsync("servicedetails:all");
            await _serviceDetailCache.InvalidateAsync($"servicedetail:{id}");
            await _serviceDetailCache.InvalidateAsync($"servicedetails:manufacturer:{oldManufacturer}");
            await _serviceDetailCache.InvalidateAsync($"servicedetails:manufacturer:{dto.Manufacturer}");
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _unitOfWork.ServiceDetail.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("ServiceDetail not found");

            _unitOfWork.ServiceDetail.DeleteAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _serviceDetailCache.InvalidateAsync("servicedetails:all");
            await _serviceDetailCache.InvalidateAsync($"servicedetail:{id}");
            await _serviceDetailCache.InvalidateAsync($"servicedetails:manufacturer:{existing.Manufacturer}");
        }

        public async Task<List<ServiceDetailDto>> GetByManufacturerAsync(string manufacturer)
        {
            var key = $"servicedetails:manufacturer:{manufacturer}";
            return await _serviceDetailCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var details = await _unitOfWork.ServiceDetail.GetByManufacturerAsync(manufacturer);
                    return _mapper.Map<List<ServiceDetailDto>>(details);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<ServiceDetailDto>();
        }
    }

}
