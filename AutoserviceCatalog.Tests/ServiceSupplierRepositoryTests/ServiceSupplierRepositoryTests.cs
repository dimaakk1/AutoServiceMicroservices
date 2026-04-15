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

namespace AutoserviceCatalog.Tests.ServiceSupplierRepositoryTests
{
    public class ServiceSupplierRepositoryTests : IDisposable
    {
        private readonly CarServiceContext _context;
        private readonly ServiceSupplierRepository _sut;

        public ServiceSupplierRepositoryTests()
        {
            _context = DbContextFactory.Create();
            _sut = new ServiceSupplierRepository(_context);
        }

        [Fact]
        public async Task GetSuppliersByServiceIdAsync_ReturnsSuppliers()
        {
            var service = new Service
            {
                ServiceId = 1,
                Name = "Oil Change",
                Price = 100,
                CategoryId = 1
            };

            var supplier1 = new Supplier
            {
                SupplierId = 1,
                Name = "Bosch",
                Phone = "111"
            };

            var supplier2 = new Supplier
            {
                SupplierId = 2,
                Name = "Denso",
                Phone = "222"
            };

            var link1 = new ServiceSupplier
            {
                ServiceId = 1,
                SupplierId = 1,
                Service = service,
                Supplier = supplier1
            };

            var link2 = new ServiceSupplier
            {
                ServiceId = 1,
                SupplierId = 2,
                Service = service,
                Supplier = supplier2
            };

            await _context.Services.AddAsync(service);
            await _context.Suppliers.AddRangeAsync(supplier1, supplier2);
            await _context.ServiceSupplier.AddRangeAsync(link1, link2);
            await _context.SaveChangesAsync();

            var result = await _sut.GetSuppliersByServiceIdAsync(1);

            result.Should().HaveCount(2);
            result.Should().Contain(s => s.Name == "Bosch");
            result.Should().Contain(s => s.Name == "Denso");
        }

        [Fact]
        public async Task GetSuppliersByServiceIdAsync_NoLinks_ReturnsEmpty()
        {
            var service = new Service
            {
                ServiceId = 1,
                Name = "Brake Repair",
                Price = 200,
                CategoryId = 1
            };

            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();

            var result = await _sut.GetSuppliersByServiceIdAsync(1);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetServicesBySupplierIdAsync_ReturnsServices()
        {
            var service1 = new Service
            {
                ServiceId = 1,
                Name = "Oil Change",
                Price = 100,
                CategoryId = 1
            };

            var service2 = new Service
            {
                ServiceId = 2,
                Name = "Brake Repair",
                Price = 200,
                CategoryId = 1
            };

            var supplier = new Supplier
            {
                SupplierId = 1,
                Name = "Bosch",
                Phone = "111"
            };

            var link1 = new ServiceSupplier
            {
                ServiceId = 1,
                SupplierId = 1,
                Service = service1,
                Supplier = supplier
            };

            var link2 = new ServiceSupplier
            {
                ServiceId = 2,
                SupplierId = 1,
                Service = service2,
                Supplier = supplier
            };

            await _context.Services.AddRangeAsync(service1, service2);
            await _context.Suppliers.AddAsync(supplier);
            await _context.ServiceSupplier.AddRangeAsync(link1, link2);
            await _context.SaveChangesAsync();

            var result = await _sut.GetServicesBySupplierIdAsync(1);

            result.Should().HaveCount(2);
            result.Should().Contain(s => s.Name == "Oil Change");
            result.Should().Contain(s => s.Name == "Brake Repair");
        }

        [Fact]
        public async Task GetServicesBySupplierIdAsync_NoLinks_ReturnsEmpty()
        {
            var supplier = new Supplier
            {
                SupplierId = 1,
                Name = "Bosch",
                Phone = "111"
            };

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            var result = await _sut.GetServicesBySupplierIdAsync(1);

            result.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
