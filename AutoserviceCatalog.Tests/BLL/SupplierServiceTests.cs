using AutoMapper;
using AutoServiceCatalog.BLL.Cache;
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


namespace AutoserviceCatalog.Tests.BLL
{
    public class SupplierServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<ISupplierRepository> _repo;
        private readonly Mock<IMapper> _mapper;

        private readonly Mock<ITwoLevelCacheService<List<SupplierDto>>> _cache;

        private readonly SupplierService _sut;

        public SupplierServiceTests()
        {
            _uow = new Mock<IUnitOfWork>();
            _repo = new Mock<ISupplierRepository>();
            _mapper = new Mock<IMapper>();

            _cache = new Mock<ITwoLevelCacheService<List<SupplierDto>>>();

            _uow.Setup(u => u.Suppliers).Returns(_repo.Object);

            _sut = new SupplierService(
                _uow.Object,
                _mapper.Object,
                _cache.Object
            );
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCachedData()
        {
            _cache.Setup(c => c.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<List<SupplierDto>>>>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<TimeSpan>()))
                .ReturnsAsync(new List<SupplierDto>
                {
                    new SupplierDto { Name = "Test" }
                });

            var result = await _sut.GetAllAsync();

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsSupplier()
        {
            _cache.Setup(c => c.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<List<SupplierDto>>>>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<TimeSpan>()))
                .ReturnsAsync(new List<SupplierDto>
                {
                    new SupplierDto { Name = "Test" }
                });

            var result = await _sut.GetByIdAsync(1);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_AddsAndInvalidatesCache()
        {
            var dto = new SupplierCreateDto
            {
                Name = "New",
                Phone = "123"
            };

            var entity = new Supplier
            {
                SupplierId = 1,
                Name = "New",
                Phone = "123"
            };

            _mapper.Setup(m => m.Map<Supplier>(dto)).Returns(entity);
            _mapper.Setup(m => m.Map<SupplierDto>(entity)).Returns(new SupplierDto { Name = "New" });

            var result = await _sut.CreateAsync(dto);

            result.Should().NotBeNull();

            _repo.Verify(r => r.AddAsync(entity), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);

            _cache.Verify(c => c.InvalidateAsync("suppliers:all"), Times.Once);
            _cache.Verify(c => c.InvalidateAsync("supplier:1"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ThrowsException()
        {
            _repo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Supplier?)null);

            var dto = new SupplierCreateDto
            {
                Name = "Test",
                Phone = "123"
            };

            Func<Task> act = () => _sut.UpdateAsync(1, dto);

            await act.Should().ThrowAsync<Exception>();

            _repo.Verify(r => r.UpdateAsync(It.IsAny<Supplier>()), Times.Never);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Valid_UpdatesAndInvalidates()
        {
            var entity = new Supplier
            {
                SupplierId = 1,
                Name = "Old",
                Phone = "000"
            };

            _repo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            var dto = new SupplierCreateDto
            {
                Name = "New",
                Phone = "111"
            };

            await _sut.UpdateAsync(1, dto);

            entity.Name.Should().Be("New");

            _repo.Verify(r => r.UpdateAsync(entity), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);

            _cache.Verify(c => c.InvalidateAsync("suppliers:all"), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ThrowsException()
        {
            _repo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Supplier?)null);

            Func<Task> act = () => _sut.DeleteAsync(1);

            await act.Should().ThrowAsync<Exception>();

            _repo.Verify(r => r.DeleteAsync(It.IsAny<Supplier>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_Valid_DeletesAndInvalidates()
        {
            var entity = new Supplier
            {
                SupplierId = 1,
                Name = "Test"
            };

            _repo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            await _sut.DeleteAsync(1);

            _repo.Verify(r => r.DeleteAsync(entity), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);

            _cache.Verify(c => c.InvalidateAsync("suppliers:all"), Times.Once);
        }

        [Fact]
        public async Task SearchByName_ReturnsList()
        {
            _cache.Setup(c => c.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<List<SupplierDto>>>>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<TimeSpan>()))
                .ReturnsAsync(new List<SupplierDto>
                {
                    new SupplierDto { Name = "A" }
                });

            var result = await _sut.SearchByNameAsync("A");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetSupplierWithServices_ReturnsSupplier()
        {
            _cache.Setup(c => c.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<List<SupplierDto>>>>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<TimeSpan>()))
                .ReturnsAsync(new List<SupplierDto>
                {
                    new SupplierDto { Name = "Test" }
                });

            var result = await _sut.GetSupplierWithServicesAsync(1);

            result.Should().NotBeNull();
        }
    }
}
