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


namespace AutoserviceCatalog.Tests.ServiceDetailRepositoryTests
{
    public class ServiceDetailRepositoryTests : IDisposable
    {
        private readonly CarServiceContext _context;
        private readonly ServiceDetailRepository _sut;

        public ServiceDetailRepositoryTests()
        {
            _context = DbContextFactory.Create();
            _sut = new ServiceDetailRepository(_context);
        }

        [Fact]
        public async Task GetByManufacturerAsync_ExistingManufacturer_ReturnsResultsWithService()
        {
            var service = new Service
            {
                ServiceId = 1,
                Name = "Oil Change",
                Price = 100,
                CategoryId = 1
            };

            var details = new ServiceDetail
            {
                ServiceDetailId = 1,
                Manufacturer = "Bosch",
                Warranty = "2 years",
                ServiceId = 1,
                Service = service
            };

            await _context.Services.AddAsync(service);
            await _context.ServiceDetails.AddAsync(details);
            await _context.SaveChangesAsync();

            var result = await _sut.GetByManufacturerAsync("Bosch");

            result.Should().HaveCount(1);
            result.First().Manufacturer.Should().Be("Bosch");
            result.First().Service.Should().NotBeNull();
            result.First().Service.Name.Should().Be("Oil Change");
        }

        [Fact]
        public async Task GetByManufacturerAsync_CaseInsensitiveSearch_WorksCorrectly()
        {
            var service = new Service
            {
                ServiceId = 1,
                Name = "Brake Repair",
                Price = 150,
                CategoryId = 1
            };

            var details = new ServiceDetail
            {
                ServiceDetailId = 1,
                Manufacturer = "Bosch",
                Warranty = "2 years",
                ServiceId = 1,
                Service = service
            };

            await _context.Services.AddAsync(service);
            await _context.ServiceDetails.AddAsync(details);
            await _context.SaveChangesAsync();

            var result = await _sut.GetByManufacturerAsync("bosch");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByManufacturerAsync_NoMatch_ReturnsEmpty()
        {
            var service = new Service
            {
                ServiceId = 1,
                Name = "Engine Repair",
                Price = 200,
                CategoryId = 1
            };

            var details = new ServiceDetail
            {
                ServiceDetailId = 1,
                Manufacturer = "Bosch",
                Warranty = "2 years",
                ServiceId = 1,
                Service = service
            };

            await _context.Services.AddAsync(service);
            await _context.ServiceDetails.AddAsync(details);
            await _context.SaveChangesAsync();

            var result = await _sut.GetByManufacturerAsync("Denso");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByManufacturerAsync_IncludeService_LoadsNavigationProperty()
        {
            var service = new Service
            {
                ServiceId = 1,
                Name = "Suspension Repair",
                Price = 300,
                CategoryId = 1
            };

            var details = new ServiceDetail
            {
                ServiceDetailId = 1,
                Manufacturer = "KYB",
                Warranty = "3 years",
                ServiceId = 1,
                Service = service
            };

            await _context.Services.AddAsync(service);
            await _context.ServiceDetails.AddAsync(details);
            await _context.SaveChangesAsync();

            var result = await _sut.GetByManufacturerAsync("KYB");

            result.First().Service.Should().NotBeNull();
            result.First().Service.Name.Should().Be("Suspension Repair");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
