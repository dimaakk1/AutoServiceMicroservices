using AutoMapper;
using AutoServiceCatalog.BLL.DTO;
using AutoServiceCatalog.BLL.Services;
using AutoServiceCatalog.DAL.Entities;
using AutoServiceCatalog.DAL.Repositories.Intarfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoServiceCatalog.DAL.UOW;
using AutoServiceCatalog.DAL.Entities;
using AutoServiceCatalog.BLL.Cache;

namespace AutoserviceCatalog.Tests.BLL
{
    public class CategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;

        private readonly Mock<ITwoLevelCacheService<List<CategoryDto>>> _categoryCache;
        private readonly Mock<ITwoLevelCacheService<List<ServiceDto>>> _servicesCache;

        private readonly Mock<AutoServiceCatalog.DAL.Repositories.Intarfaces.ICategoryRepository> _mockCategoryRepo;

        private readonly CategoryService _sut;

        public CategoryServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            _categoryCache = new Mock<ITwoLevelCacheService<List<CategoryDto>>>();
            _servicesCache = new Mock<ITwoLevelCacheService<List<ServiceDto>>>();

            _mockCategoryRepo = new Mock<AutoServiceCatalog.DAL.Repositories.Intarfaces.ICategoryRepository>();

            _mockUow.Setup(u => u.Categories).Returns(_mockCategoryRepo.Object);

            _sut = new CategoryService(
                _mockUow.Object,
                _mockMapper.Object,
                _categoryCache.Object,
                _servicesCache.Object
            );
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsMappedCategories()
        {
            var dtoList = new List<CategoryDto>
            {
                new CategoryDto { Name = "Test" }
            };

            _categoryCache
                .Setup(c => c.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<List<CategoryDto>>>>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<TimeSpan?>()))
                .ReturnsAsync(dtoList);

            var result = await _sut.GetAllCategoriesAsync();

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Test");
        }

        [Fact]
        public async Task GetServicesByCategoryNameAsync_ReturnsServices()
        {
            var dtoList = new List<ServiceDto>
            {
                new ServiceDto { Name = "Oil Change" }
            };

            _servicesCache
                .Setup(c => c.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<List<ServiceDto>>>>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<TimeSpan?>()))
                .ReturnsAsync(dtoList);

            var result = await _sut.GetServicesByCategoryNameAsync("Test");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task AddCategoryAsync_Valid_AddsAndSaves()
        {
            var dto = new CategoryDto { Name = "New" };
            var entity = new Category { CategoryId = 1, Name = "New" };

            _mockMapper.Setup(m => m.Map<Category>(dto)).Returns(entity);
            _mockMapper.Setup(m => m.Map<CategoryDto>(entity)).Returns(dto);

            var result = await _sut.AddCategoryAsync(dto);

            result.Should().NotBeNull();

            _mockCategoryRepo.Verify(r => r.AddAsync(entity), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
            _categoryCache.Verify(c => c.InvalidateAsync("categories:all"), Times.Once);
        }

        [Fact]
        public async Task AddCategoryAsync_EmptyName_ThrowsException()
        {
            var dto = new CategoryDto { Name = "" };

            Func<Task> act = () => _sut.AddCategoryAsync(dto);

            await act.Should().ThrowAsync<ArgumentException>();

            _mockCategoryRepo.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_Existing_DeletesAndInvalidatesCache()
        {
            var category = new Category { CategoryId = 1, Name = "Test" };

            _mockCategoryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

            await _sut.DeleteAsync(1);

            _mockCategoryRepo.Verify(r => r.DeleteAsync(category), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);

            _categoryCache.Verify(c => c.InvalidateAsync("categories:all"), Times.Once);
            _servicesCache.Verify(c => c.InvalidateAsync("services:byCategory:Test"), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NotFound_DoesNothing()
        {
            _mockCategoryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Category?)null);

            await _sut.DeleteAsync(1);

            _mockCategoryRepo.Verify(r => r.DeleteAsync(It.IsAny<Category>()), Times.Never);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Valid_UpdatesAndInvalidatesCache()
        {
            var existing = new Category { CategoryId = 1, Name = "Old" };
            var dto = new CategoryDto { Name = "New" };

            _mockCategoryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

            await _sut.UpdateAsync(1, dto);

            existing.Name.Should().Be("New");

            _mockCategoryRepo.Verify(r => r.UpdateAsync(existing), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);

            _categoryCache.Verify(c => c.InvalidateAsync("categories:all"), Times.Once);
            _servicesCache.Verify(c => c.InvalidateAsync("services:byCategory:Old"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NotFound_ThrowsException()
        {
            _mockCategoryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Category?)null);

            var dto = new CategoryDto { Name = "New" };

            Func<Task> act = () => _sut.UpdateAsync(1, dto);

            await act.Should().ThrowAsync<Exception>();

            _mockCategoryRepo.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_EmptyName_ThrowsException()
        {
            var existing = new Category { CategoryId = 1, Name = "Old" };

            _mockCategoryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

            var dto = new CategoryDto { Name = "" };

            Func<Task> act = () => _sut.UpdateAsync(1, dto);

            await act.Should().ThrowAsync<ArgumentException>();

            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
    }
}
