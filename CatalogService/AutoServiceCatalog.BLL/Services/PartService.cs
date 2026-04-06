using AutoMapper;
using AutoServiceCatalog.BLL.Cache;
using AutoServiceCatalog.BLL.DTO;
using AutoServiceCatalog.BLL.Services.Interfaces;
using AutoServiceCatalog.DAL.Entities;
using AutoServiceCatalog.DAL.QueryParametrs;
using AutoServiceCatalog.DAL.Specefication;
using AutoServiceCatalog.DAL.UOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TwoLevelCacheService<List<ServiceDto>> _servicesCache;

        public ServiceService(IUnitOfWork unitOfWork, IMapper mapper, TwoLevelCacheService<List<ServiceDto>> servicesCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _servicesCache = servicesCache;
        }

        public async Task<List<ServiceDto>> GetAllAsync()
        {
            var key = "services:all";
            return await _servicesCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var services = await _unitOfWork.Services.GetAllAsync();
                    return _mapper.Map<List<ServiceDto>>(services);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<ServiceDto>();
        }

        public async Task<ServiceDto> GetByIdAsync(int id)
        {
            var key = $"service:{id}";
            return await _servicesCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var service = await _unitOfWork.Services.GetByIdAsync(id);
                    if (service == null)
                        return null;
                    return new List<ServiceDto> { _mapper.Map<ServiceDto>(service) };
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ).ContinueWith(t => t.Result?.FirstOrDefault()) ?? null!;
        }

        public async Task<ServiceDto> CreateAsync(ServiceCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Назва послуги не може бути порожньою!");

            if (dto.Price <= 0)
                throw new ArgumentException("Ціна повинна бути більшою за 0!");

            var service = _mapper.Map<Service>(dto);
            await _unitOfWork.Services.AddAsync(service);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _servicesCache.InvalidateAsync("services:all");
            await _servicesCache.InvalidateAsync($"service:{service.ServiceId}");

            return _mapper.Map<ServiceDto>(service);
        }

        public async Task UpdateAsync(int id, ServiceCreateDto dto)
        {
            var existing = await _unitOfWork.Services.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Послугу не знайдено");

            existing.Name = dto.Name;
            existing.Price = dto.Price;
            existing.CategoryId = dto.CategoryId;

            _unitOfWork.Services.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _servicesCache.InvalidateAsync("services:all");
            await _servicesCache.InvalidateAsync($"service:{id}");
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _unitOfWork.Services.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Послугу не знайдено");

            _unitOfWork.Services.DeleteAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _servicesCache.InvalidateAsync("services:all");
            await _servicesCache.InvalidateAsync($"service:{id}");
        }

        public async Task<List<ServiceDto>> SearchByNameAsync(string keyword)
        {
            var key = $"services:search:{keyword}";
            return await _servicesCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var result = await _unitOfWork.Services.SearchByNameAsync(keyword);
                    return _mapper.Map<List<ServiceDto>>(result);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<ServiceDto>();
        }

        public async Task<List<ServiceDto>> GetServicesAbovePriceAsync(decimal price)
        {
            var key = $"services:above:{price}";
            return await _servicesCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var services = await _unitOfWork.Services.GetServicesAbovePriceAsync(price);
                    return _mapper.Map<List<ServiceDto>>(services);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<ServiceDto>();
        }

        public async Task<List<ServiceDto>> GetServicesBelowPriceAsync(decimal price)
        {
            var key = $"services:below:{price}";
            return await _servicesCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var services = await _unitOfWork.Services.GetServicesBelowPriceAsync(price);
                    return _mapper.Map<List<ServiceDto>>(services);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<ServiceDto>();
        }

        public async Task<PagedResult<Service>> GetServicesAsync(PartQueryParameters parameters)
        {
            return await _unitOfWork.Services.GetServicesAsync(parameters);
        }
    }

}
