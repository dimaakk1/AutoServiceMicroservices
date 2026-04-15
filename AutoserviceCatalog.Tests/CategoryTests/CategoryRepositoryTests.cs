using AutoServiceCatalog.DAL.Db;
using AutoServiceCatalog.DAL.Entities;
using AutoServiceCatalog.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoserviceCatalog.Tests.Helpers;
using FluentAssertions;

namespace AutoserviceCatalog.Tests.CategoryTests
{
    public class CategoryRepositoryTests : IDisposable
    {
        private readonly CarServiceContext _context;
        private readonly CategoryRepository _sut;

        public CategoryRepositoryTests()
        {
            _context = DbContextFactory.Create();
            _sut = new CategoryRepository(_context);
        }

        [Fact]
        public async Task GetServicesByCategoryNameAsync_ExistingCategoryWithServices_ReturnsServices()
        {
            var category = new Category
            {
                CategoryId = 1,
                Name = "Engine"
            };

            var services = new List<Service>
            {
                new Service { ServiceId = 1, Name = "Oil Change", Price = 100, CategoryId = 1, Category = category },
                new Service { ServiceId = 2, Name = "Filter Change", Price = 50, CategoryId = 1, Category = category }
            };

            await _context.Categories.AddAsync(category);
            await _context.Services.AddRangeAsync(services);
            await _context.SaveChangesAsync();

            var result = await _sut.GetServicesByCategoryNameAsync("Engine");

            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.CategoryId == 1);
        }

        [Fact]
        public async Task GetServicesByCategoryNameAsync_CategoryExistsWithoutServices_ReturnsEmpty()
        {
            var category = new Category
            {
                CategoryId = 1,
                Name = "EmptyCategory"
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var result = await _sut.GetServicesByCategoryNameAsync("EmptyCategory");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetServicesByCategoryNameAsync_CategoryDoesNotExist_ReturnsEmpty()
        {
            var result = await _sut.GetServicesByCategoryNameAsync("NotExisting");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetServicesByCategoryNameAsync_NullOrEmpty_ReturnsEmpty()
        {
            var result1 = await _sut.GetServicesByCategoryNameAsync(null!);
            var result2 = await _sut.GetServicesByCategoryNameAsync(string.Empty);

            result1.Should().BeEmpty();
            result2.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
