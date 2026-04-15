using AutoServiceCatalog.DAL.Db;
using AutoServiceCatalog.DAL.Entities;
using AutoServiceCatalog.DAL.QueryParametrs;
using AutoServiceCatalog.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoserviceCatalog.Tests.Helpers;
using FluentAssertions;

namespace AutoserviceCatalog.Tests.ServiceRepositoryTests
{
    public class ServiceRepositoryTests : IDisposable
    {
        private readonly CarServiceContext _context;
        private readonly ServiceRepository _sut;

        public ServiceRepositoryTests()
        {
            _context = DbContextFactory.Create();
            _sut = new ServiceRepository(_context);
        }

        [Fact]
        public async Task GetServicesAbovePriceAsync_ReturnsOnlyHigherPrices()
        {
            var services = new List<Service>
            {
                new Service { ServiceId = 1, Name = "A", Price = 50, CategoryId = 1 },
                new Service { ServiceId = 2, Name = "B", Price = 150, CategoryId = 1 },
                new Service { ServiceId = 3, Name = "C", Price = 200, CategoryId = 1 }
            };

            await _context.Services.AddRangeAsync(services);
            await _context.SaveChangesAsync();

            var result = await _sut.GetServicesAbovePriceAsync(100);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.Price > 100);
        }

        [Fact]
        public async Task GetServicesBelowPriceAsync_ReturnsOnlyLowerPrices()
        {
            var services = new List<Service>
            {
                new Service { ServiceId = 1, Name = "A", Price = 50, CategoryId = 1 },
                new Service { ServiceId = 2, Name = "B", Price = 150, CategoryId = 1 },
                new Service { ServiceId = 3, Name = "C", Price = 200, CategoryId = 1 }
            };

            await _context.Services.AddRangeAsync(services);
            await _context.SaveChangesAsync();

            var result = await _sut.GetServicesBelowPriceAsync(100);

            result.Should().HaveCount(1);
            result.Should().OnlyContain(s => s.Price < 100);
        }

        [Fact]
        public async Task SearchByNameAsync_ReturnsMatchingServices()
        {
            var services = new List<Service>
            {
                new Service { ServiceId = 1, Name = "Oil Change", Price = 50, CategoryId = 1 },
                new Service { ServiceId = 2, Name = "Oil Filter", Price = 80, CategoryId = 1 },
                new Service { ServiceId = 3, Name = "Brake Repair", Price = 200, CategoryId = 1 }
            };

            await _context.Services.AddRangeAsync(services);
            await _context.SaveChangesAsync();

            var result = await _sut.SearchByNameAsync("Oil");

            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.Name.Contains("Oil"));
        }

        [Fact]
        public async Task SearchByNameAsync_NoMatches_ReturnsEmpty()
        {
            await _context.Services.AddAsync(
                new Service { ServiceId = 1, Name = "Engine Repair", Price = 100, CategoryId = 1 }
            );
            await _context.SaveChangesAsync();

            var result = await _sut.SearchByNameAsync("XYZ");

            result.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
