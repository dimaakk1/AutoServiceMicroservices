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
    public class ServiceDetailControllerTests
    {
        private readonly Mock<IServiceDetailService> _serviceMock;
        private readonly ServiceDetailController _sut;

        public ServiceDetailControllerTests()
        {
            _serviceMock = new Mock<IServiceDetailService>();
            _sut = new ServiceDetailController(_serviceMock.Object);
        }


        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var list = new List<ServiceDetailDto>
            {
                new ServiceDetailDto { ServiceDetailId = 1 }
            };

            _serviceMock.Setup(s => s.GetAllAsync())
                .ReturnsAsync(list);

            var result = await _sut.GetAll();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(list);
        }


        [Fact]
        public async Task GetById_Existing_ReturnsOk()
        {
            var dto = new ServiceDetailDto { ServiceDetailId = 1 };

            _serviceMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(dto);

            var result = await _sut.GetById(1);

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(999))
                .ReturnsAsync((ServiceDetailDto?)null);

            var result = await _sut.GetById(999);

            result.Should().BeOfType<NotFoundResult>();
        }


        [Fact]
        public async Task Create_ReturnsCreated()
        {
            var dto = new ServiceDetailCreateDto();
            var created = new ServiceDetailDto { ServiceDetailId = 5 };

            _serviceMock.Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(created);

            var result = await _sut.Create(dto);

            var createdResult = result.Should()
                .BeOfType<CreatedAtActionResult>()
                .Subject;

            createdResult.Value.Should().BeEquivalentTo(created);
            createdResult.ActionName.Should().Be(nameof(ServiceDetailController.GetById));
        }


        [Fact]
        public async Task Update_ReturnsNoContent()
        {
            var dto = new ServiceDetailCreateDto();

            _serviceMock.Setup(s => s.UpdateAsync(1, dto))
                .Returns(Task.CompletedTask);

            var result = await _sut.Update(1, dto);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Update_WhenNotFound_ReturnsNotFound()
        {
            var dto = new ServiceDetailCreateDto();

            _serviceMock.Setup(s => s.UpdateAsync(1, dto))
                .ThrowsAsync(new Exception("not found"));

            var result = await _sut.Update(1, dto);

            result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task Delete_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1))
                .Returns(Task.CompletedTask);

            var result = await _sut.Delete(1);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_WhenNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1))
                .ThrowsAsync(new Exception("not found"));

            var result = await _sut.Delete(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task GetByManufacturer_ReturnsOk_WhenDataExists()
        {
            var list = new List<ServiceDetailDto>
            {
                new ServiceDetailDto { ServiceDetailId = 1 }
            };

            _serviceMock.Setup(s => s.GetByManufacturerAsync("BMW"))
                .ReturnsAsync(list);

            var result = await _sut.GetByManufacturer("BMW");

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(list);
        }

        [Fact]
        public async Task GetByManufacturer_ReturnsNotFound_WhenEmpty()
        {
            _serviceMock.Setup(s => s.GetByManufacturerAsync("BMW"))
                .ReturnsAsync(new List<ServiceDetailDto>());

            var result = await _sut.GetByManufacturer("BMW");

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
