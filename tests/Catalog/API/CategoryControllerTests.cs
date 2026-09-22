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
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _serviceMock;
        private readonly CategoryController _sut;

        public CategoryControllerTests()
        {
            _serviceMock = new Mock<ICategoryService>();

            _sut = new CategoryController(_serviceMock.Object);

            _sut.ControllerContext = new ControllerContext();
        }


        [Fact]
        public async Task GetAllCategories_ReturnsOk()
        {
            var list = new List<CategoryDto>
            {
                new CategoryDto { Name = "A" }
            };

            _serviceMock.Setup(s => s.GetAllCategoriesAsync())
                .ReturnsAsync(list);

            var result = await _sut.GetAllCategories();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;

            ok.Value.Should().BeEquivalentTo(list);
        }


        [Fact]
        public async Task AddCategory_Valid_ReturnsOk()
        {
            var dto = new CategoryDto { Name = "Test" };

            _serviceMock.Setup(s => s.AddCategoryAsync(dto))
                .ReturnsAsync(dto);

            var result = await _sut.AddCategory(dto);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;

            ok.Value.Should().BeEquivalentTo(dto);

            _serviceMock.Verify(s => s.AddCategoryAsync(dto), Times.Once);
        }

        [Fact]
        public async Task AddCategory_Exception_ReturnsBadRequest()
        {
            var dto = new CategoryDto { Name = "Test" };

            _serviceMock.Setup(s => s.AddCategoryAsync(dto))
                .ThrowsAsync(new Exception("Error"));

            var result = await _sut.AddCategory(dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }


        [Fact]
        public async Task Delete_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1))
                .Returns(Task.CompletedTask);

            var result = await _sut.Delete(1);

            result.Should().BeOfType<NoContentResult>();

            _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
        }


        [Fact]
        public async Task Update_Valid_ReturnsNoContent()
        {
            var dto = new CategoryDto { Name = "New" };

            _serviceMock.Setup(s => s.UpdateAsync(1, dto))
                .Returns(Task.CompletedTask);

            var result = await _sut.UpdateCategory(1, dto);

            result.Should().BeOfType<NoContentResult>();

            _serviceMock.Verify(s => s.UpdateAsync(1, dto), Times.Once);
        }

        [Fact]
        public async Task Update_Exception_ReturnsBadRequest()
        {
            var dto = new CategoryDto { Name = "New" };

            _serviceMock.Setup(s => s.UpdateAsync(1, dto))
                .ThrowsAsync(new Exception("Error"));

            var result = await _sut.UpdateCategory(1, dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }


        [Fact]
        public async Task GetServicesByCategoryName_Valid_ReturnsOk()
        {
            var list = new List<ServiceDto>
            {
                new ServiceDto { Name = "Oil" }
            };

            _serviceMock.Setup(s => s.GetServicesByCategoryNameAsync("Car"))
                .ReturnsAsync(list);

            var result = await _sut.GetServicesByCategoryName("Car");

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;

            ok.Value.Should().BeEquivalentTo(list);
        }

        [Fact]
        public async Task GetServicesByCategoryName_Empty_ReturnsBadRequest()
        {
            var result = await _sut.GetServicesByCategoryName("");

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetServicesByCategoryName_Exception_ReturnsBadRequest()
        {
            _serviceMock.Setup(s => s.GetServicesByCategoryNameAsync("Car"))
                .ThrowsAsync(new Exception("Error"));

            var result = await _sut.GetServicesByCategoryName("Car");

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
