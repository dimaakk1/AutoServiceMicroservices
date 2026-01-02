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
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TwoLevelCacheService<List<CategoryDto>> _categoryCache;
        private readonly TwoLevelCacheService<List<PartDto>> _partsCache;

        public CategoryService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            TwoLevelCacheService<List<CategoryDto>> categoryCache,
            TwoLevelCacheService<List<PartDto>> partsCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _categoryCache = categoryCache;
            _partsCache = partsCache;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _categoryCache.GetOrCreateAsync(
                key: "categories:all",
                factory: async () =>
                {
                    var categories = await _unitOfWork.Categories.GetAllAsync();
                    return _mapper.Map<List<CategoryDto>>(categories);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<CategoryDto>();
        }

        public async Task<List<PartDto>> GetPartsByCategoryNameAsync(string categoryName)
        {
            var key = $"parts:byCategory:{categoryName}";
            return await _partsCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var parts = await _unitOfWork.Categories.GetPartsByCategoryNameAsync(categoryName);
                    return _mapper.Map<List<PartDto>>(parts);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<PartDto>();
        }

        public async Task<CategoryDto> AddCategoryAsync(CategoryDto categoryDto)
        {
            if (string.IsNullOrWhiteSpace(categoryDto.Name))
                throw new ArgumentException("Назва категорії не може бути порожньою!");

            var category = _mapper.Map<Category>(categoryDto);
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідуємо кеш після зміни
            await _categoryCache.InvalidateAsync("categories:all");

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Categories.GetByIdAsync(id);
            if (entity != null)
            {
                _unitOfWork.Categories.DeleteAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                // Інвалідуємо кеш
                await _categoryCache.InvalidateAsync("categories:all");
                await _partsCache.InvalidateAsync($"parts:byCategory:{entity.Name}");
            }
        }

        public async Task UpdateAsync(int id, CategoryDto dto)
        {
            var existing = await _unitOfWork.Categories.GetByIdAsync(id);

            if (existing == null)
            {
                throw new Exception("Категорію не знайдено");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Назва категорії не може бути порожньою!");
            }

            var oldName = existing.Name;
            existing.Name = dto.Name;

            _unitOfWork.Categories.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            // Інвалідуємо кеш
            await _categoryCache.InvalidateAsync("categories:all");
            await _partsCache.InvalidateAsync($"parts:byCategory:{oldName}");
            await _partsCache.InvalidateAsync($"parts:byCategory:{dto.Name}");
        }
    }
}
