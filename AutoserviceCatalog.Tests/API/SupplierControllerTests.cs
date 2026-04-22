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
    public class SupplierControllerTests
    {
        private readonly Mock<ISupplierService> _serviceMock;
        private readonly SupplierController _sut;

        public SupplierControllerTests()
        {
            _serviceMock = new Mock<ISupplierService>();
            _sut = new SupplierController(_serviceMock.Object);

            _sut.ControllerContext = new ControllerContext();
        }


        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var list = new List<SupplierDto>
            {
                new SupplierDto { SupplierId = 1, Name = "Test" }
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
            var dto = new SupplierDto { SupplierId = 1, Name = "Test" };

            _serviceMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(dto);

            var result = await _sut.GetById(1);

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync((SupplierDto?)null);

            var result = await _sut.GetById(1);

            result.Should().BeOfType<NotFoundResult>();
        }


        [Fact]
        public async Task Create_Valid_ReturnsCreated()
        {
            var dto = new SupplierCreateDto { Name = "Test", Phone = "123" };
            var created = new SupplierDto { SupplierId = 1, Name = "Test" };

            _serviceMock.Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(created);

            var result = await _sut.Create(dto);

            var createdResult = result.Should()
                .BeOfType<CreatedAtActionResult>()
                .Subject;

            createdResult.Value.Should().BeEquivalentTo(created);
            createdResult.ActionName.Should().Be(nameof(SupplierController.GetById));
        }

        [Fact]
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {
            _sut.ModelState.AddModelError("Name", "Required");

            var dto = new SupplierCreateDto();

            var result = await _sut.Create(dto);

            result.Should().BeOfType<BadRequestObjectResult>();

            _serviceMock.Verify(s => s.CreateAsync(It.IsAny<SupplierCreateDto>()), Times.Never);
        }


        [Fact]
        public async Task Update_Valid_ReturnsNoContent()
        {
            var dto = new SupplierCreateDto { Name = "Test", Phone = "123" };

            _serviceMock.Setup(s => s.UpdateAsync(1, dto))
                .Returns(Task.CompletedTask);

            var result = await _sut.Update(1, dto);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Update_InvalidModel_ReturnsBadRequest()
        {
            _sut.ModelState.AddModelError("Name", "Required");

            var dto = new SupplierCreateDto();

            var result = await _sut.Update(1, dto);

            result.Should().BeOfType<BadRequestObjectResult>();

            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<SupplierCreateDto>()), Times.Never);
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
        public async Task GetSupplierWithServices_ReturnsOk()
        {
            var dto = new SupplierDto { SupplierId = 1, Name = "Test" };

            _serviceMock.Setup(s => s.GetSupplierWithServicesAsync(1))
                .ReturnsAsync(dto);

            var result = await _sut.GetSupplierWithServices(1);

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetSupplierWithServices_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetSupplierWithServicesAsync(1))
                .ThrowsAsync(new Exception("not found"));

            var result = await _sut.GetSupplierWithServices(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task Search_ReturnsOk()
        {
            var list = new List<SupplierDto>
            {
                new SupplierDto { SupplierId = 1, Name = "Test" }
            };

            _serviceMock.Setup(s => s.SearchByNameAsync("test"))
                .ReturnsAsync(list);

            var result = await _sut.Search("test");

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(list);
        }
    }
}
