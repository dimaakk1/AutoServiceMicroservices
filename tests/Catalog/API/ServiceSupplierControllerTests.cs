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
    public class ServiceSupplierControllerTests
    {
        private readonly Mock<IServiceSupplierService> _serviceMock;
        private readonly ServiceSupplierController _sut;

        public ServiceSupplierControllerTests()
        {
            _serviceMock = new Mock<IServiceSupplierService>();
            _sut = new ServiceSupplierController(_serviceMock.Object);
        }


        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var list = new List<ServiceSupplierDto>
            {
                new ServiceSupplierDto { ServiceId = 1, SupplierId = 1 }
            };

            _serviceMock.Setup(s => s.GetAllAsync())
                .ReturnsAsync(list);

            var result = await _sut.GetAll();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(list);
        }


        [Fact]
        public async Task GetByIds_Existing_ReturnsOk()
        {
            var dto = new ServiceSupplierDto
            {
                ServiceId = 1,
                SupplierId = 2
            };

            _serviceMock.Setup(s => s.GetByIdsAsync(1, 2))
                .ReturnsAsync(dto);

            var result = await _sut.GetByIds(1, 2);

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetByIds_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdsAsync(1, 2))
                .ReturnsAsync((ServiceSupplierDto?)null);

            var result = await _sut.GetByIds(1, 2);

            result.Should().BeOfType<NotFoundResult>();
        }


        [Fact]
        public async Task Create_ReturnsCreated()
        {
            var dto = new ServiceSupplierDto
            {
                ServiceId = 1,
                SupplierId = 2
            };

            _serviceMock.Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(dto);

            var result = await _sut.Create(dto);

            var created = result.Should()
                .BeOfType<CreatedAtActionResult>()
                .Subject;

            created.Value.Should().BeEquivalentTo(dto);
            created.ActionName.Should().Be(nameof(ServiceSupplierController.GetByIds));
        }


        [Fact]
        public async Task Delete_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1, 2))
                .Returns(Task.CompletedTask);

            var result = await _sut.Delete(1, 2);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_WhenNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1, 2))
                .ThrowsAsync(new Exception("not found"));

            var result = await _sut.Delete(1, 2);

            result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task GetSuppliersByService_ReturnsOk()
        {
            var list = new List<SupplierDto>
            {
                new SupplierDto { SupplierId = 1, Name = "Test" }
            };

            _serviceMock.Setup(s => s.GetSuppliersByServiceIdAsync(1))
                .ReturnsAsync(list);

            var result = await _sut.GetSuppliersByService(1);

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(list);
        }


        [Fact]
        public async Task GetServicesBySupplier_ReturnsOk()
        {
            var list = new List<ServiceDto>
            {
                new ServiceDto { ServiceId = 1, Name = "Test" }
            };

            _serviceMock.Setup(s => s.GetServicesBySupplierIdAsync(1))
                .ReturnsAsync(list);

            var result = await _sut.GetServicesBySupplier(1);

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(list);
        }
    }
}
