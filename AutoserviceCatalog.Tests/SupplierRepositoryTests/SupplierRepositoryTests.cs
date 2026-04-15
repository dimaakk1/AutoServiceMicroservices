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

namespace AutoserviceCatalog.Tests.SupplierRepositoryTests
{
    public class SupplierRepositoryTests : IDisposable
    {
        private readonly CarServiceContext _context;
        private readonly SupplierRepository _sut;

        public SupplierRepositoryTests()
        {
            _context = DbContextFactory.Create();
            _sut = new SupplierRepository(_context);
        }

        [Fact]
        public async Task GetSupplierWithServicesAsync_ReturnsSupplierWithServices()
        {
            var service = new Service
            {
                ServiceId = 1,
                Name = "Oil Change",
                Price = 100,
                CategoryId = 1
            };

            var supplier = new Supplier
            {
                SupplierId = 1,
                Name = "AutoParts Ltd",
                Phone = "123456",
                ServiceSuppliers = new List<ServiceSupplier>()
            };

            var serviceSupplier = new ServiceSupplier
            {
                SupplierId = 1,
                ServiceId = 1,
                Service = service,
                Supplier = supplier
            };

            supplier.ServiceSuppliers.Add(serviceSupplier);

            await _context.Services.AddAsync(service);
            await _context.Suppliers.AddAsync(supplier);
            await _context.ServiceSupplier.AddAsync(serviceSupplier);
            await _context.SaveChangesAsync();

            var result = await _sut.GetSupplierWithServicesAsync(1);

            result.Should().NotBeNull();
            result!.ServiceSuppliers.Should().NotBeEmpty();
            result.ServiceSuppliers.First().Service.Should().NotBeNull();
            result.ServiceSuppliers.First().Service.Name.Should().Be("Oil Change");
        }

        [Fact]
        public async Task GetSupplierWithServicesAsync_InvalidId_ReturnsNull()
        {
            var result = await _sut.GetSupplierWithServicesAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task SearchByNameAsync_ReturnsMatchingSuppliers()
        {
            var suppliers = new List<Supplier>
            {
                new Supplier { SupplierId = 1, Name = "Bosch Auto", Phone = "111" },
                new Supplier { SupplierId = 2, Name = "Bosch Parts", Phone = "222" },
                new Supplier { SupplierId = 3, Name = "Denso", Phone = "333" }
            };

            await _context.Suppliers.AddRangeAsync(suppliers);
            await _context.SaveChangesAsync();

            var result = await _sut.SearchByNameAsync("Bosch");

            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.Name.Contains("Bosch"));
        }

        [Fact]
        public async Task SearchByNameAsync_NoMatches_ReturnsEmpty()
        {
            await _context.Suppliers.AddAsync(
                new Supplier { SupplierId = 1, Name = "Magneti Marelli", Phone = "123" }
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
