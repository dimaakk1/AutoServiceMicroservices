using AutoServiceCatalog.API.Controllers;
using AutoServiceCatalog.BLL.DTO;
using AutoServiceCatalog.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace AutoserviceCatalog.Tests.API
{
    public class ServiceControllerTests
    {
        private readonly Mock<IServiceService> _serviceMock;
        private readonly ServiceController _sut;

        public ServiceControllerTests()
        {
            _serviceMock = new Mock<IServiceService>();
            _sut = new ServiceController(_serviceMock.Object);
        }


        [Fact]
        public async Task GetAll_ReturnsOkWithServices()
        {
            var list = new List<ServiceDto>
            {
                new ServiceDto { ServiceId = 1, Name = "Test", Price = 100 }
            };

            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(list);

            var result = await _sut.GetAll();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(list);
        }


        [Fact]
        public async Task GetById_Existing_ReturnsOk()
        {
            var dto = new ServiceDto { ServiceId = 1, Name = "Test", Price = 100 };

            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await _sut.GetById(1);

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(999))
                .ThrowsAsync(new Exception("Not found"));

            var result = await _sut.GetById(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task Create_Valid_ReturnsCreated()
        {
            var dto = new ServiceCreateDto { Name = "Test", Price = 100 };
            var created = new ServiceDto { ServiceId = 1, Name = "Test", Price = 100 };

            _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

            var result = await _sut.Create(dto);

            var createdResult = result.Should()
                .BeOfType<CreatedAtActionResult>()
                .Subject;

            createdResult.Value.Should().BeEquivalentTo(created);
            createdResult.ActionName.Should().Be(nameof(ServiceController.GetById));
        }

        [Fact]
        public async Task Create_Invalid_ReturnsBadRequest()
        {
            var dto = new ServiceCreateDto();

            _serviceMock.Setup(s => s.CreateAsync(dto))
                .ThrowsAsync(new ArgumentException("error"));

            var result = await _sut.Create(dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }


        [Fact]
        public async Task Update_Valid_ReturnsNoContent()
        {
            var dto = new ServiceCreateDto { Name = "Test", Price = 100 };

            _serviceMock.Setup(s => s.UpdateAsync(1, dto))
                .Returns(Task.CompletedTask);

            var result = await _sut.Update(1, dto);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Update_NotFound_ReturnsNotFound()
        {
            var dto = new ServiceCreateDto { Name = "Test", Price = 100 };

            _serviceMock.Setup(s => s.UpdateAsync(1, dto))
                .ThrowsAsync(new Exception("Not found"));

            var result = await _sut.Update(1, dto);

            result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task Delete_Valid_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1))
                .Returns(Task.CompletedTask);

            var result = await _sut.Delete(1);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1))
                .ThrowsAsync(new Exception("Not found"));

            var result = await _sut.Delete(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task Search_ReturnsOk()
        {
            var list = new List<ServiceDto>
            {
                new ServiceDto { ServiceId = 1, Name = "Oil" }
            };

            _serviceMock.Setup(s => s.SearchByNameAsync("oil"))
                .ReturnsAsync(list);

            var result = await _sut.Search("oil");

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(list);
        }


        [Fact]
        public async Task GetServicesAbovePrice_ReturnsOk_WhenDataExists()
        {
            var list = new List<ServiceDto>
            {
                new ServiceDto { ServiceId = 1, Price = 500 }
            };

            _serviceMock.Setup(s => s.GetServicesAbovePriceAsync(100))
                .ReturnsAsync(list);

            var result = await _sut.GetServicesAbovePrice(100);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetServicesAbovePrice_ReturnsNotFound_WhenEmpty()
        {
            _serviceMock.Setup(s => s.GetServicesAbovePriceAsync(100))
                .ReturnsAsync(new List<ServiceDto>());

            var result = await _sut.GetServicesAbovePrice(100);

            result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task GetServicesBelowPrice_ReturnsOk_WhenDataExists()
        {
            var list = new List<ServiceDto>
            {
                new ServiceDto { ServiceId = 1, Price = 50 }
            };

            _serviceMock.Setup(s => s.GetServicesBelowPriceAsync(100))
                .ReturnsAsync(list);

            var result = await _sut.GetServicesBelowPrice(100);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetServicesBelowPrice_ReturnsNotFound_WhenEmpty()
        {
            _serviceMock.Setup(s => s.GetServicesBelowPriceAsync(100))
                .ReturnsAsync(new List<ServiceDto>());

            var result = await _sut.GetServicesBelowPrice(100);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
