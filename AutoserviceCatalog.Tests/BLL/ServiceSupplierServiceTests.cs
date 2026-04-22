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
    public class ServiceSupplierServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IServiceSupplierRepository> _repo;
        private readonly Mock<IMapper> _mapper;

        private readonly Mock<ITwoLevelCacheService<List<ServiceSupplierDto>>> _serviceSupplierCache;
        private readonly Mock<ITwoLevelCacheService<List<ServiceDto>>> _servicesCache;
        private readonly Mock<ITwoLevelCacheService<List<SupplierDto>>> _suppliersCache;

        private readonly ServiceSupplierService _sut;

        public ServiceSupplierServiceTests()
        {
            _uow = new Mock<IUnitOfWork>();
            _repo = new Mock<IServiceSupplierRepository>();
            _mapper = new Mock<IMapper>();

            _serviceSupplierCache = new Mock<ITwoLevelCacheService<List<ServiceSupplierDto>>>();
            _servicesCache = new Mock<ITwoLevelCacheService<List<ServiceDto>>>();
            _suppliersCache = new Mock<ITwoLevelCacheService<List<SupplierDto>>>();

            _uow.Setup(u => u.ServiceSupplier).Returns(_repo.Object);

            _sut = new ServiceSupplierService(
                _uow.Object,
                _mapper.Object,
                _serviceSupplierCache.Object,
                _servicesCache.Object,
                _suppliersCache.Object
            );
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCachedData()
        {
            var dto = new List<ServiceSupplierDto>
            {
                new ServiceSupplierDto()
            };

            _serviceSupplierCache
                .Setup(c => c.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<List<ServiceSupplierDto>>>>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<TimeSpan>()))
                .ReturnsAsync(dto);

            var result = await _sut.GetAllAsync();

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task CreateAsync_AddsEntity_AndInvalidatesCaches()
        {
            var dto = new ServiceSupplierDto
            {
                ServiceId = 1,
                SupplierId = 2
            };

            var entity = new ServiceSupplier
            {
                ServiceId = 1,
                SupplierId = 2
            };

            _mapper.Setup(m => m.Map<ServiceSupplier>(dto)).Returns(entity);
            _mapper.Setup(m => m.Map<ServiceSupplierDto>(entity)).Returns(dto);

            var result = await _sut.CreateAsync(dto);

            result.Should().NotBeNull();

            _repo.Verify(r => r.AddAsync(entity), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);

            _serviceSupplierCache.Verify(c => c.InvalidateAsync("servicesuppliers:all"), Times.Once);
            _servicesCache.Verify(c => c.InvalidateAsync("services:supplier:2"), Times.Once);
            _suppliersCache.Verify(c => c.InvalidateAsync("suppliers:service:1"), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenExists_DeletesAndInvalidates()
        {
            var entity = new ServiceSupplier
            {
                ServiceId = 1,
                SupplierId = 2
            };

            _repo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ServiceSupplier> { entity });

            await _sut.DeleteAsync(1, 2);

            _repo.Verify(r => r.DeleteAsync(entity), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);

            _serviceSupplierCache.Verify(c => c.InvalidateAsync("servicesuppliers:all"), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ThrowsException()
        {
            _repo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ServiceSupplier>());

            Func<Task> act = () => _sut.DeleteAsync(1, 2);

            await act.Should().ThrowAsync<Exception>();

            _repo.Verify(r => r.DeleteAsync(It.IsAny<ServiceSupplier>()), Times.Never);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task GetServicesBySupplierId_ReturnsList()
        {
            var services = new List<Service>
            {
                new Service { ServiceId = 1, Name = "Oil" }
            };

            _servicesCache
                .Setup(c => c.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<List<ServiceDto>>>>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<TimeSpan>()))
                .ReturnsAsync(new List<ServiceDto> { new ServiceDto() });

            var result = await _sut.GetServicesBySupplierIdAsync(1);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetSuppliersByServiceId_ReturnsList()
        {
            _suppliersCache
                .Setup(c => c.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<List<SupplierDto>>>>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<TimeSpan>()))
                .ReturnsAsync(new List<SupplierDto> { new SupplierDto() });

            var result = await _sut.GetSuppliersByServiceIdAsync(1);

            result.Should().HaveCount(1);
        }
    }
}
