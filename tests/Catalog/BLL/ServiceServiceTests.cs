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
    public class ServiceServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ITwoLevelCacheService<List<ServiceDto>>> _cache;

        private readonly Mock<IServiceRepository> _repo;
        private readonly Mock<ICategoryRepository> _categoryRepo;

        private readonly ServiceService _sut;

        public ServiceServiceTests()
        {
            _uow = new Mock<IUnitOfWork>();
            _mapper = new Mock<IMapper>();
            _cache = new Mock<ITwoLevelCacheService<List<ServiceDto>>>();

            _repo = new Mock<IServiceRepository>();
            _categoryRepo = new Mock<ICategoryRepository>();

            _uow.Setup(x => x.Services).Returns(_repo.Object);
            _uow.Setup(x => x.Categories).Returns(_categoryRepo.Object);

            _sut = new ServiceService(
                _uow.Object,
                _mapper.Object,
                _cache.Object
            );
        }


        [Fact]
        public async Task GetAllAsync_ReturnsCachedData()
        {
            var expected = new List<ServiceDto>
            {
                new ServiceDto { Name = "A" }
            };

            _cache.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<ServiceDto>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>()))
                .ReturnsAsync(expected);

            var result = await _sut.GetAllAsync();

            result.Should().HaveCount(1);
        }


        [Fact]
        public async Task GetByIdAsync_ReturnsItem()
        {
            var dto = new ServiceDto { Name = "Oil Change" };

            _cache.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<ServiceDto>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>()))
                .ReturnsAsync(new List<ServiceDto> { dto });

            var result = await _sut.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Oil Change");
        }


        [Fact]
        public async Task CreateAsync_Valid_AddsService()
        {
            var dto = new ServiceCreateDto
            {
                Name = "Oil",
                Price = 100,
                CategoryName = "Maintenance"
            };

            var category = new Category { CategoryId = 1, Name = "Maintenance" };

            var mappedDto = new ServiceDto { ServiceId = 1, Name = "Oil", Price = 100 };

            _categoryRepo.Setup(x => x.GetByNameAsync(dto.CategoryName))
                .ReturnsAsync(category);
            
            // Setup the mapper to map the Service to ServiceDto
            _mapper.Setup(x => x.Map<ServiceDto>(It.IsAny<Service>()))
                .Returns((Service s) => new ServiceDto 
                { 
                    ServiceId = s.ServiceId, 
                    Name = s.Name, 
                    Price = s.Price 
                });

            // Setup AddAsync to set the ServiceId
            _repo.Setup(x => x.AddAsync(It.IsAny<Service>()))
                .Callback<Service>(s => s.ServiceId = 1)
                .Returns(Task.CompletedTask);

            var result = await _sut.CreateAsync(dto);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Oil");

            _repo.Verify(x => x.AddAsync(It.IsAny<Service>()), Times.Once);
            _uow.Verify(x => x.SaveChangesAsync(), Times.Once);

            _cache.Verify(x => x.InvalidateAsync("services:all"), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_InvalidName_Throws()
        {
            var dto = new ServiceCreateDto
            {
                Name = "",
                Price = 100
            };

            Func<Task> act = () => _sut.CreateAsync(dto);

            await act.Should().ThrowAsync<ArgumentException>();

            _repo.Verify(x => x.AddAsync(It.IsAny<Service>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_InvalidPrice_Throws()
        {
            var dto = new ServiceCreateDto
            {
                Name = "Test",
                Price = 0,
                CategoryName = "Maintenance"
            };

            var category = new Category { CategoryId = 1, Name = "Maintenance" };
            _categoryRepo.Setup(x => x.GetByNameAsync(dto.CategoryName))
                .ReturnsAsync(category);

            Func<Task> act = () => _sut.CreateAsync(dto);

            await act.Should().ThrowAsync<ArgumentException>();

            _repo.Verify(x => x.AddAsync(It.IsAny<Service>()), Times.Never);
        }


        [Fact]
        public async Task UpdateAsync_Valid_UpdatesService()
        {
            var existing = new Service
            {
                ServiceId = 1,
                Name = "Old",
                Price = 50,
                CategoryId = 1
            };

            _repo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existing);

            var dto = new ServiceCreateDto
            {
                Name = "New",
                Price = 200,
                CategoryName = "Repairs"
            };

            var category = new Category { CategoryId = 2, Name = "Repairs" };
            _categoryRepo.Setup(x => x.GetByNameAsync(dto.CategoryName))
                .ReturnsAsync(category);

            await _sut.UpdateAsync(1, dto);

            existing.Name.Should().Be("New");
            existing.Price.Should().Be(200);

            _repo.Verify(x => x.UpdateAsync(existing), Times.Once);
            _uow.Verify(x => x.SaveChangesAsync(), Times.Once);

            _cache.Verify(x => x.InvalidateAsync("services:all"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NotFound_Throws()
        {
            _repo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Service?)null);

            Func<Task> act = () => _sut.UpdateAsync(1, new ServiceCreateDto());

            await act.Should().ThrowAsync<Exception>();

            _repo.Verify(x => x.UpdateAsync(It.IsAny<Service>()), Times.Never);
        }


        [Fact]
        public async Task DeleteAsync_Valid_Deletes()
        {
            var entity = new Service
            {
                ServiceId = 1,
                Name = "Test"
            };

            _repo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(entity);

            await _sut.DeleteAsync(1);

            _repo.Verify(x => x.DeleteAsync(entity), Times.Once);
            _uow.Verify(x => x.SaveChangesAsync(), Times.Once);

            _cache.Verify(x => x.InvalidateAsync("services:all"), Times.Once);
        }


        [Fact]
        public async Task SearchByName_ReturnsResults()
        {
            var dto = new List<ServiceDto>
            {
                new ServiceDto { Name = "Oil" }
            };

            _cache.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<ServiceDto>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>()))
                .ReturnsAsync(dto);

            var result = await _sut.SearchByNameAsync("Oil");

            result.Should().HaveCount(1);
        }


        [Fact]
        public async Task GetServicesAbovePrice_ReturnsList()
        {
            var dto = new List<ServiceDto>
            {
                new ServiceDto { Price = 200 }
            };

            _cache.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<ServiceDto>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>()))
                .ReturnsAsync(dto);

            var result = await _sut.GetServicesAbovePriceAsync(100);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetServicesBelowPrice_ReturnsList()
        {
            var dto = new List<ServiceDto>
            {
                new ServiceDto { Price = 50 }
            };

            _cache.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<ServiceDto>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>()))
                .ReturnsAsync(dto);

            var result = await _sut.GetServicesBelowPriceAsync(100);

            result.Should().HaveCount(1);
        }
    }
}
