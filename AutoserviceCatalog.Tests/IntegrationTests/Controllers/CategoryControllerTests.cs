using AutoserviceCatalog.Tests.IntegrationTests.Infrastructure;
using AutoServiceCatalog.BLL.DTO;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceCatalog.Tests.IntegrationTests.Controllers
{
    public class CategoryControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CategoryControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/Catalog/Category");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var data = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
            data.Should().NotBeNull();
        }

        [Fact]
        public async Task AddCategory_Valid_ReturnsOk()
        {
            var dto = new CategoryDto
            {
                Name = "Test Category"
            };

            var response = await _client.PostAsJsonAsync("/api/Catalog/Category", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<CategoryDto>();
            result!.Name.Should().Be("Test Category");
        }

        [Fact]
        public async Task AddCategory_Invalid_ReturnsBadRequest()
        {
            var dto = new CategoryDto
            {
                Name = "" 
            };

            var response = await _client.PostAsJsonAsync("/api/Catalog/Category", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsNoContent()
        {
            var dto = new CategoryDto
            {
                Name = "Updated Name"
            };

            var response = await _client.PutAsJsonAsync("/api/Catalog/Category/1", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsNoContent()
        {
            var response = await _client.DeleteAsync("/api/Catalog/Category/1");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task GetServicesByCategoryName_ReturnsOk()
        {
            var response = await _client.GetAsync(
                "/api/Catalog/Category/services/byName?categoryName=ТО");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetServicesByCategoryName_Empty_ReturnsBadRequest()
        {
            var response = await _client.GetAsync(
                "/api/Catalog/Category/services/byName?categoryName=");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
