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
    public class ServiceSupplierService : IServiceSupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TwoLevelCacheService<List<ServiceSupplierDto>> _serviceSupplierCache;
        private readonly TwoLevelCacheService<List<ServiceDto>> _servicesCache;
        private readonly TwoLevelCacheService<List<SupplierDto>> _suppliersCache;

        public ServiceSupplierService(IUnitOfWork unitOfWork, IMapper mapper, TwoLevelCacheService<List<ServiceSupplierDto>> serviceSupplierCache, TwoLevelCacheService<List<ServiceDto>> servicesCache, TwoLevelCacheService<List<SupplierDto>> suppliersCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceSupplierCache = serviceSupplierCache;
            _servicesCache = servicesCache;
            _suppliersCache = suppliersCache;
        }

        public async Task<List<ServiceSupplierDto>> GetAllAsync()
        {
            var key = "servicesuppliers:all";
            return await _serviceSupplierCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var entities = await _unitOfWork.ServiceSupplier.GetAllAsync();
                    return _mapper.Map<List<ServiceSupplierDto>>(entities);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<ServiceSupplierDto>();
        }

        public async Task<ServiceSupplierDto?> GetByIdsAsync(int serviceId, int supplierId)
        {
            var key = $"servicesupplier:{serviceId}:{supplierId}";
            return await _serviceSupplierCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var all = await _unitOfWork.ServiceSupplier.GetAllAsync();
                    var entity = all.FirstOrDefault(ss => ss.ServiceId == serviceId && ss.SupplierId == supplierId);
                    return entity != null ? new List<ServiceSupplierDto> { _mapper.Map<ServiceSupplierDto>(entity) } : new List<ServiceSupplierDto>();
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<ServiceSupplierDto> CreateAsync(ServiceSupplierDto dto)
        {
            var entity = _mapper.Map<ServiceSupplier>(dto);
            await _unitOfWork.ServiceSupplier.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _serviceSupplierCache.InvalidateAsync("servicesuppliers:all");
            await _serviceSupplierCache.InvalidateAsync($"servicesupplier:{entity.ServiceId}:{entity.SupplierId}");
            await _servicesCache.InvalidateAsync($"services:supplier:{entity.SupplierId}");
            await _suppliersCache.InvalidateAsync($"suppliers:service:{entity.ServiceId}");

            return _mapper.Map<ServiceSupplierDto>(entity);
        }

        public async Task DeleteAsync(int serviceId, int supplierId)
        {
            var all = await _unitOfWork.ServiceSupplier.GetAllAsync();
            var entity = all.FirstOrDefault(ss => ss.ServiceId == serviceId && ss.SupplierId == supplierId);

            if (entity == null)
                throw new Exception("ServiceSupplier link not found");

            _unitOfWork.ServiceSupplier.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _serviceSupplierCache.InvalidateAsync("servicesuppliers:all");
            await _serviceSupplierCache.InvalidateAsync($"servicesupplier:{serviceId}:{supplierId}");
            await _servicesCache.InvalidateAsync($"services:supplier:{supplierId}");
            await _suppliersCache.InvalidateAsync($"suppliers:service:{serviceId}");
        }

        public async Task<List<ServiceDto>> GetServicesBySupplierIdAsync(int supplierId)
        {
            var key = $"services:supplier:{supplierId}";
            return await _servicesCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var services = await _unitOfWork.ServiceSupplier.GetServicesBySupplierIdAsync(supplierId);
                    return _mapper.Map<List<ServiceDto>>(services);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<ServiceDto>();
        }

        public async Task<List<SupplierDto>> GetSuppliersByServiceIdAsync(int serviceId)
        {
            var key = $"suppliers:service:{serviceId}";
            return await _suppliersCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var suppliers = await _unitOfWork.ServiceSupplier.GetSuppliersByServiceIdAsync(serviceId);
                    return _mapper.Map<List<SupplierDto>>(suppliers);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<SupplierDto>();
        }
    }   

}
