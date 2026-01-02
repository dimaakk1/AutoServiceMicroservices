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
    public class PartSupplierService : IPartSupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TwoLevelCacheService<List<PartSupplierDto>> _partSupplierCache;
        private readonly TwoLevelCacheService<List<PartDto>> _partsCache;
        private readonly TwoLevelCacheService<List<SupplierDto>> _suppliersCache;

        public PartSupplierService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            TwoLevelCacheService<List<PartSupplierDto>> partSupplierCache,
            TwoLevelCacheService<List<PartDto>> partsCache,
            TwoLevelCacheService<List<SupplierDto>> suppliersCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _partSupplierCache = partSupplierCache;
            _partsCache = partsCache;
            _suppliersCache = suppliersCache;
        }

        public async Task<List<PartSupplierDto>> GetAllAsync()
        {
            return await _partSupplierCache.GetOrCreateAsync(
                key: "partsuppliers:all",
                factory: async () =>
                {
                    var entities = await _unitOfWork.PartSupplier.GetAllAsync();
                    return _mapper.Map<List<PartSupplierDto>>(entities);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<PartSupplierDto>();
        }

        public async Task<PartSupplierDto?> GetByIdsAsync(int partId, int supplierId)
        {
            var key = $"partsupplier:{partId}:{supplierId}";
            return await _partSupplierCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var all = await _unitOfWork.PartSupplier.GetAllAsync();
                    var entity = all.FirstOrDefault(ps => ps.PartId == partId && ps.SupplierId == supplierId);
                    return entity != null ? new List<PartSupplierDto> { _mapper.Map<PartSupplierDto>(entity) } : new List<PartSupplierDto>();
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<PartSupplierDto> CreateAsync(PartSupplierDto dto)
        {
            var entity = _mapper.Map<PartSupplier>(dto);
            await _unitOfWork.PartSupplier.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _partSupplierCache.InvalidateAsync("partsuppliers:all");
            await _partSupplierCache.InvalidateAsync($"partsupplier:{entity.PartId}:{entity.SupplierId}");
            await _partsCache.InvalidateAsync($"parts:supplier:{entity.SupplierId}");
            await _suppliersCache.InvalidateAsync($"suppliers:part:{entity.PartId}");

            return _mapper.Map<PartSupplierDto>(entity);
        }

        public async Task DeleteAsync(int partId, int supplierId)
        {
            var all = await _unitOfWork.PartSupplier.GetAllAsync();
            var entity = all.FirstOrDefault(ps => ps.PartId == partId && ps.SupplierId == supplierId);

            if (entity == null)
                throw new Exception("PartSupplier link not found");

            _unitOfWork.PartSupplier.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _partSupplierCache.InvalidateAsync("partsuppliers:all");
            await _partSupplierCache.InvalidateAsync($"partsupplier:{partId}:{supplierId}");
            await _partsCache.InvalidateAsync($"parts:supplier:{supplierId}");
            await _suppliersCache.InvalidateAsync($"suppliers:part:{partId}");
        }

        public async Task<List<PartDto>> GetPartsBySupplierIdAsync(int supplierId)
        {
            var key = $"parts:supplier:{supplierId}";
            return await _partsCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var parts = await _unitOfWork.PartSupplier.GetPartsBySupplierIdAsync(supplierId);
                    return _mapper.Map<List<PartDto>>(parts);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<PartDto>();
        }

        public async Task<List<SupplierDto>> GetSuppliersByPartIdAsync(int partId)
        {
            var key = $"suppliers:part:{partId}";
            return await _suppliersCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var suppliers = await _unitOfWork.PartSupplier.GetSuppliersByPartIdAsync(partId);
                    return _mapper.Map<List<SupplierDto>>(suppliers);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<SupplierDto>();
        }
    }

}
