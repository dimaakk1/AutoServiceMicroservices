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
    public class PartService : IPartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TwoLevelCacheService<List<PartDto>> _partsCache;

        public PartService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            TwoLevelCacheService<List<PartDto>> partsCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _partsCache = partsCache;
        }

        public async Task<PagedResult<Part>> GetPartsAsync(PartQueryParameters parameters)
        {
            // Під цей метод кешування складніше, залишаємо без кешу
            return await _unitOfWork.Parts.GetPartsAsync(parameters);
        }

        public async Task<IEnumerable<PartDto>> GetAllAsync()
        {
            return await _partsCache.GetOrCreateAsync(
                key: "parts:all",
                factory: async () =>
                {
                    var parts = await _unitOfWork.Parts.GetAllAsync();
                    return _mapper.Map<List<PartDto>>(parts);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<PartDto>();
        }

        public async Task<PartDto> GetByIdAsync(int id)
        {
            var key = $"part:{id}";
            return await _partsCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var part = await _unitOfWork.Parts.GetByIdAsync(id);
                    if (part == null)
                        return null;
                    return new List<PartDto> { _mapper.Map<PartDto>(part) };
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ).ContinueWith(t => t.Result?.FirstOrDefault()) ?? null!;
        }

        public async Task<PartDto> CreateAsync(PartCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Назва запчастини не може бути порожньою!");

            if (dto.Price <= 0)
                throw new ArgumentException("Ціна повинна бути більшою за 0!");

            var part = _mapper.Map<Part>(dto);
            await _unitOfWork.Parts.AddAsync(part);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _partsCache.InvalidateAsync("parts:all");
            await _partsCache.InvalidateAsync($"part:{part.PartId}");

            return _mapper.Map<PartDto>(part);
        }

        public async Task UpdateAsync(int id, PartCreateDto dto)
        {
            var existing = await _unitOfWork.Parts.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Запчастину не знайдено");

            existing.Name = dto.Name;
            existing.Price = dto.Price;
            existing.CategoryId = dto.CategoryId;

            _unitOfWork.Parts.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _partsCache.InvalidateAsync("parts:all");
            await _partsCache.InvalidateAsync($"part:{id}");
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _unitOfWork.Parts.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Запчастину не знайдено");

            _unitOfWork.Parts.DeleteAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідація кешу
            await _partsCache.InvalidateAsync("parts:all");
            await _partsCache.InvalidateAsync($"part:{id}");
        }

        public async Task<List<PartDto>> SearchByNameAsync(string keyword)
        {
            var key = $"parts:search:{keyword}";
            return await _partsCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var result = await _unitOfWork.Parts.SearchByNameAsync(keyword);
                    return _mapper.Map<List<PartDto>>(result);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<PartDto>();
        }

        public async Task<List<PartDto>> GetPartsAbovePriceAsync(decimal price)
        {
            var key = $"parts:above:{price}";
            return await _partsCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var parts = await _unitOfWork.Parts.GetPartsAbovePriceAsync(price);
                    return _mapper.Map<List<PartDto>>(parts);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<PartDto>();
        }

        public async Task<List<PartDto>> GetPartsBelowPriceAsync(decimal price)
        {
            var key = $"parts:below:{price}";
            return await _partsCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var parts = await _unitOfWork.Parts.GetPartsBelowPriceAsync(price);
                    return _mapper.Map<List<PartDto>>(parts);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<PartDto>();
        }
    }

}
