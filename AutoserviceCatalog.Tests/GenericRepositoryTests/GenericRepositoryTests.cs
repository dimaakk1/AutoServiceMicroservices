using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoserviceCatalog.Tests.Helpers;
using Xunit;
using FluentAssertions;
using AutoServiceCatalog.DAL.Repositories;
using AutoServiceCatalog.DAL.Entities;
using AutoServiceCatalog.DAL.Db;

namespace AutoserviceCatalog.Tests.GenericRepositoryTests
{

    public class GenericRepositoryTests : IDisposable
    {
        private readonly CarServiceContext _context;
        private readonly GenericRepository<Service> _sut;

        public GenericRepositoryTests()
        {
            _context = DbContextFactory.Create();
            _sut = new GenericRepository<Service>(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsService()
        {
            var category = new Category { CategoryId = 1, Name = "Test" };

            var service = new Service
            {
                ServiceId = 1,
                Name = "Oil Change",
                Price = 100,
                CategoryId = 1,
                Category = category
            };

            await _context.Categories.AddAsync(category);
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();

            var result = await _sut.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Oil Change");
        }

        [Fact]
        public async Task GetByIdAsync_NotExisting_ReturnsNull()
        {
            var result = await _sut.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_WithData_ReturnsAll()
        {
            var category = new Category { CategoryId = 1, Name = "Test" };

            var services = new List<Service>
        {
            new Service { ServiceId = 1, Name = "A", Price = 10, CategoryId = 1, Category = category },
            new Service { ServiceId = 2, Name = "B", Price = 20, CategoryId = 1, Category = category }
        };

            await _context.Categories.AddAsync(category);
            await _context.Services.AddRangeAsync(services);
            await _context.SaveChangesAsync();

            var result = await _sut.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Empty_ReturnsEmpty()
        {
            var result = await _sut.GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AddAsync_AddsService()
        {
            var category = new Category { CategoryId = 1, Name = "Test" };

            var service = new Service
            {
                ServiceId = 1,
                Name = "New Service",
                Price = 50,
                CategoryId = 1,
                Category = category
            };

            await _context.Categories.AddAsync(category);
            await _sut.AddAsync(service);
            await _context.SaveChangesAsync();

            _context.Services.Should().HaveCount(1);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesService()
        {
            var category = new Category { CategoryId = 1, Name = "Test" };

            var service = new Service
            {
                ServiceId = 1,
                Name = "Old",
                Price = 50,
                CategoryId = 1,
                Category = category
            };

            await _context.Categories.AddAsync(category);
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();

            service.Name = "Updated";
            _sut.UpdateAsync(service);
            await _context.SaveChangesAsync();

            var result = await _context.Services.FindAsync(1);

            result!.Name.Should().Be("Updated");
        }

        [Fact]
        public async Task DeleteAsync_RemovesService()
        {
            var category = new Category { CategoryId = 1, Name = "Test" };

            var service = new Service
            {
                ServiceId = 1,
                Name = "ToDelete",
                Price = 50,
                CategoryId = 1,
                Category = category
            };

            await _context.Categories.AddAsync(category);
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();

            _sut.DeleteAsync(service);
            await _context.SaveChangesAsync();

            _context.Services.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
