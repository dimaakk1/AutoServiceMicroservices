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
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TwoLevelCacheService<List<SupplierDto>> _suppliersCache;

        public SupplierService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            TwoLevelCacheService<List<SupplierDto>> suppliersCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _suppliersCache = suppliersCache;
        }

        public async Task<IEnumerable<SupplierDto>> GetAllAsync()
        {
            return await _suppliersCache.GetOrCreateAsync(
                key: "suppliers:all",
                factory: async () =>
                {
                    var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
                    return _mapper.Map<List<SupplierDto>>(suppliers);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<SupplierDto>();
        }

        public async Task<SupplierDto> GetByIdAsync(int id)
        {
            var key = $"supplier:{id}";
            return await _suppliersCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                    if (supplier == null) return null;
                    return new List<SupplierDto> { _mapper.Map<SupplierDto>(supplier) };
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ).ContinueWith(t => t.Result?.FirstOrDefault()) ?? null!;
        }

        public async Task<SupplierDto> CreateAsync(SupplierCreateDto dto)
        {
            var entity = _mapper.Map<Supplier>(dto);
            await _unitOfWork.Suppliers.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідуємо кеш
            await _suppliersCache.InvalidateAsync("suppliers:all");
            await _suppliersCache.InvalidateAsync($"supplier:{entity.SupplierId}");

            return _mapper.Map<SupplierDto>(entity);
        }

        public async Task UpdateAsync(int id, SupplierCreateDto dto)
        {
            var existing = await _unitOfWork.Suppliers.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Постачальника не знайдено");

            existing.Name = dto.Name;
            existing.Phone = dto.Phone;

            _unitOfWork.Suppliers.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідуємо кеш
            await _suppliersCache.InvalidateAsync("suppliers:all");
            await _suppliersCache.InvalidateAsync($"supplier:{id}");
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _unitOfWork.Suppliers.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Постачальника не знайдено");

            _unitOfWork.Suppliers.DeleteAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідуємо кеш
            await _suppliersCache.InvalidateAsync("suppliers:all");
            await _suppliersCache.InvalidateAsync($"supplier:{id}");
        }

        public async Task<List<SupplierDto>> SearchByNameAsync(string keyword)
        {
            var key = $"suppliers:search:{keyword}";
            return await _suppliersCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var result = await _unitOfWork.Suppliers.SearchByNameAsync(keyword);
                    return _mapper.Map<List<SupplierDto>>(result);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<SupplierDto>();
        }

        public async Task<SupplierDto> GetSupplierWithPartsAsync(int id)
        {
            var key = $"supplier:withParts:{id}";
            return await _suppliersCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var supplier = await _unitOfWork.Suppliers.GetSupplierWithPartsAsync(id);
                    if (supplier == null) return null;
                    return new List<SupplierDto> { _mapper.Map<SupplierDto>(supplier) };
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ).ContinueWith(t => t.Result?.FirstOrDefault()) ?? null!;
        }
    }

}
