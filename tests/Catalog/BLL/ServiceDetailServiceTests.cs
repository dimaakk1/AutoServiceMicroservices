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
    public class ServiceDetailServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ITwoLevelCacheService<List<ServiceDetailDto>>> _cache;

        private readonly Mock<IServiceDetailRepository> _repo;
        private readonly Mock<IServiceRepository> _serviceRepo;

        private readonly ServiceDetailService _sut;

        public ServiceDetailServiceTests()
        {
            _uow = new Mock<IUnitOfWork>();
            _mapper = new Mock<IMapper>();
            _cache = new Mock<ITwoLevelCacheService<List<ServiceDetailDto>>>();

            _repo = new Mock<IServiceDetailRepository>();
            _serviceRepo = new Mock<IServiceRepository>();

            _uow.Setup(x => x.ServiceDetail).Returns(_repo.Object);
            _uow.Setup(x => x.Services).Returns(_serviceRepo.Object);

            _sut = new ServiceDetailService(
                _uow.Object,
                _mapper.Object,
                _cache.Object
            );
        }


        [Fact]
        public async Task GetAllAsync_ReturnsFromCache()
        {
            var expected = new List<ServiceDetailDto>
            {
                new ServiceDetailDto()
            };

            _cache.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<ServiceDetailDto>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>()))
                .ReturnsAsync(expected);

            var result = await _sut.GetAllAsync();

            result.Should().HaveCount(1);
        }


        [Fact]
        public async Task GetByIdAsync_ReturnsItem()
        {
            var entity = new ServiceDetail
            {
                ServiceDetailId = 1,
                Manufacturer = "BMW"
            };

            var dto = new ServiceDetailDto
            {
                ServiceDetailId = 1,
                Manufacturer = "BMW"
            };

            _cache.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<ServiceDetailDto>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>()))
                .ReturnsAsync(new List<ServiceDetailDto> { dto });

            var result = await _sut.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Manufacturer.Should().Be("BMW");
        }


        [Fact]
        public async Task CreateAsync_Valid_AddsEntity()
        {
            var dto = new ServiceDetailCreateDto
            {
                ServiceId = 1,
                Manufacturer = "BMW",
                Warranty = "1y"
            };

            var service = new Service { ServiceId = 1 };

            var entity = new ServiceDetail
            {
                ServiceDetailId = 10,
                Manufacturer = "BMW",
                Warranty = "1y",
                ServiceId = 1
            };

            _serviceRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(service);

            _mapper.Setup(x => x.Map<ServiceDetail>(dto))
                .Returns(entity);

            _mapper.Setup(x => x.Map<ServiceDetailDto>(entity))
                .Returns(new ServiceDetailDto { Manufacturer = "BMW" });

            var result = await _sut.CreateAsync(dto);

            result.Should().NotBeNull();

            _repo.Verify(x => x.AddAsync(entity), Times.Once);
            _uow.Verify(x => x.SaveChangesAsync(), Times.Once);

            _cache.Verify(x => x.InvalidateAsync("servicedetails:all"), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ServiceNotFound_Throws()
        {
            var dto = new ServiceDetailCreateDto { ServiceId = 99 };

            _serviceRepo.Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Service?)null);

            Func<Task> act = () => _sut.CreateAsync(dto);

            await act.Should().ThrowAsync<Exception>();

            _repo.Verify(x => x.AddAsync(It.IsAny<ServiceDetail>()), Times.Never);
        }


        [Fact]
        public async Task UpdateAsync_Valid_UpdatesEntity()
        {
            var existing = new ServiceDetail
            {
                ServiceDetailId = 1,
                Manufacturer = "Old",
                Warranty = "1y"
            };

            var dto = new ServiceDetailCreateDto
            {
                Manufacturer = "New",
                Warranty = "2y"
            };

            _repo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existing);

            await _sut.UpdateAsync(1, dto);

            existing.Manufacturer.Should().Be("New");

            _repo.Verify(x => x.UpdateAsync(existing), Times.Once);
            _uow.Verify(x => x.SaveChangesAsync(), Times.Once);

            _cache.Verify(x => x.InvalidateAsync("servicedetails:all"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NotFound_Throws()
        {
            _repo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((ServiceDetail?)null);

            Func<Task> act = () => _sut.UpdateAsync(1, new ServiceDetailCreateDto());

            await act.Should().ThrowAsync<Exception>();

            _repo.Verify(x => x.UpdateAsync(It.IsAny<ServiceDetail>()), Times.Never);
        }


        [Fact]
        public async Task DeleteAsync_Valid_Deletes()
        {
            var entity = new ServiceDetail
            {
                ServiceDetailId = 1,
                Manufacturer = "BMW"
            };

            _repo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(entity);

            await _sut.DeleteAsync(1);

            _repo.Verify(x => x.DeleteAsync(entity), Times.Once);
            _uow.Verify(x => x.SaveChangesAsync(), Times.Once);

            _cache.Verify(x => x.InvalidateAsync("servicedetails:all"), Times.Once);
        }


        [Fact]
        public async Task GetByManufacturer_ReturnsList()
        {
            var dto = new List<ServiceDetailDto>
            {
                new ServiceDetailDto { Manufacturer = "BMW" }
            };

            _cache.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<ServiceDetailDto>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>()))
                .ReturnsAsync(dto);

            var result = await _sut.GetByManufacturerAsync("BMW");

            result.Should().HaveCount(1);
        }
    }
}
