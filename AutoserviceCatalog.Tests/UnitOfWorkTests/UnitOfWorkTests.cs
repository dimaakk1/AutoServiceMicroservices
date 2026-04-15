using AutoServiceCatalog.DAL.Db;
using AutoServiceCatalog.DAL.Repositories;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoserviceCatalog.Tests.Helpers;
using FluentAssertions;
using AutoServiceCatalog.DAL.UOW;
using AutoServiceCatalog.DAL.Entities;

namespace AutoserviceCatalog.Tests.UnitOfWorkTests
{
    public class UnitOfWorkTests : IDisposable
    {
        private readonly CarServiceContext _context;

        public UnitOfWorkTests()
        {
            _context = DbContextFactory.Create();
        }

        [Fact]
        public void UnitOfWork_Repositories_UseSameContext()
        {
            var uow = new UnitOfWork(
                _context,
                new ServiceRepository(_context),
                new CategoryRepository(_context),
                new SupplierRepository(_context),
                new ServiceSupplierRepository(_context),
                new ServiceDetailRepository(_context)
            );

            var contextFromServiceRepo = GetPrivateContext(uow.Services);
            var contextFromCategoryRepo = GetPrivateContext(uow.Categories);

            contextFromServiceRepo.Should().BeSameAs(contextFromCategoryRepo);
        }

        [Fact]
        public async Task SaveChangesAsync_PersistsAllChanges()
        {
            var uow = new UnitOfWork(
                _context,
                new ServiceRepository(_context),
                new CategoryRepository(_context),
                new SupplierRepository(_context),
                new ServiceSupplierRepository(_context),
                new ServiceDetailRepository(_context)
            );

            var category = new Category
            {
                CategoryId = 1,
                Name = "Test"
            };

            var service = new Service
            {
                ServiceId = 1,
                Name = "Oil Change",
                Price = 100,
                CategoryId = 1,
                Category = category
            };

            await uow.Categories.AddAsync(category);
            await uow.Services.AddAsync(service);
            await uow.SaveChangesAsync();

            _context.Categories.Should().HaveCount(1);
            _context.Services.Should().HaveCount(1);
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            var uow = new UnitOfWork(
                _context,
                new ServiceRepository(_context),
                new CategoryRepository(_context),
                new SupplierRepository(_context),
                new ServiceSupplierRepository(_context),
                new ServiceDetailRepository(_context)
            );

            Action act = () => uow.Dispose();

            act.Should().NotThrow();
        }

        private CarServiceContext GetPrivateContext(object repo)
        {
            var field = repo.GetType()
                .BaseType!
                .GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            return (CarServiceContext)field!.GetValue(repo)!;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
