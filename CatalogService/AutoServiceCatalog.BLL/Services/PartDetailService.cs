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
    public class PartDetailService : IPartDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TwoLevelCacheService<List<PartDetailDto>> _partDetailCache;

        public PartDetailService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            TwoLevelCacheService<List<PartDetailDto>> partDetailCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _partDetailCache = partDetailCache;
        }

        public async Task<List<PartDetailDto>> GetAllAsync()
        {
            return await _partDetailCache.GetOrCreateAsync(
                key: "partdetails:all",
                factory: async () =>
                {
                    var entities = await _unitOfWork.PartDetail.GetAllAsync();
                    return _mapper.Map<List<PartDetailDto>>(entities);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<PartDetailDto>();
        }

        public async Task<PartDetailDto?> GetByIdAsync(int id)
        {
            var key = $"partdetail:{id}";
            return await _partDetailCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var entity = await _unitOfWork.PartDetail.GetByIdAsync(id);
                    return entity != null ? new List<PartDetailDto> { _mapper.Map<PartDetailDto>(entity) } : new List<PartDetailDto>();
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<PartDetailDto> CreateAsync(PartDetailCreateDto dto)
        {
            var part = await _unitOfWork.Parts.GetByIdAsync(dto.PartId);
            if (part == null)
                throw new Exception("Part not found");

            var entity = _mapper.Map<PartDetail>(dto);
            entity.PartId = dto.PartId;

            await _unitOfWork.PartDetail.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _partDetailCache.InvalidateAsync("partdetails:all");
            await _partDetailCache.InvalidateAsync($"partdetail:{entity.PartDetailId}");
            await _partDetailCache.InvalidateAsync($"partdetails:manufacturer:{entity.Manufacturer}");

            return _mapper.Map<PartDetailDto>(entity);
        }

        public async Task UpdateAsync(int id, PartDetailCreateDto dto)
        {
            var existing = await _unitOfWork.PartDetail.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("PartDetail not found");

            var oldManufacturer = existing.Manufacturer;

            existing.Manufacturer = dto.Manufacturer;
            existing.Warranty = dto.Warranty;

            _unitOfWork.PartDetail.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _partDetailCache.InvalidateAsync("partdetails:all");
            await _partDetailCache.InvalidateAsync($"partdetail:{id}");
            await _partDetailCache.InvalidateAsync($"partdetails:manufacturer:{oldManufacturer}");
            await _partDetailCache.InvalidateAsync($"partdetails:manufacturer:{dto.Manufacturer}");
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _unitOfWork.PartDetail.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("PartDetail not found");

            _unitOfWork.PartDetail.DeleteAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _partDetailCache.InvalidateAsync("partdetails:all");
            await _partDetailCache.InvalidateAsync($"partdetail:{id}");
            await _partDetailCache.InvalidateAsync($"partdetails:manufacturer:{existing.Manufacturer}");
        }

        public async Task<List<PartDetailDto>> GetByManufacturerAsync(string manufacturer)
        {
            var key = $"partdetails:manufacturer:{manufacturer}";
            return await _partDetailCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var details = await _unitOfWork.PartDetail.GetByManufacturerAsync(manufacturer);
                    return _mapper.Map<List<PartDetailDto>>(details);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<PartDetailDto>();
        }
    }

}
