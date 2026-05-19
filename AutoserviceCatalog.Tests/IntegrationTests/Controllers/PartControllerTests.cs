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
    public class ServiceControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ServiceControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/Catalog/Service");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var data = await response.Content.ReadFromJsonAsync<List<ServiceDto>>();
            data.Should().NotBeNull();
            data!.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetById_Existing_ReturnsOk()
        {
            var allResponse = await _client.GetAsync("/api/Catalog/Service");
            allResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var all = await allResponse.Content.ReadFromJsonAsync<List<ServiceDto>>();
            all.Should().NotBeNull();
            all!.Should().NotBeEmpty();

            var id = all.First().ServiceId;

            var response = await _client.GetAsync($"/api/Catalog/Service/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var data = await response.Content.ReadFromJsonAsync<ServiceDto>();
            data.Should().NotBeNull();
            data!.ServiceId.Should().Be(id);
        }

        [Fact]
        public async Task GetById_NotExisting_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/Catalog/Service/99999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_Valid_ReturnsCreated()
        {
            var dto = new ServiceCreateDto
            {
                Name = "Integration Test Service",
                Price = 150,
                CategoryName = "Maintenance"
            };

            var response = await _client.PostAsJsonAsync("/api/Catalog/Service", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await response.Content.ReadFromJsonAsync<ServiceDto>();
            created.Should().NotBeNull();
            created!.Name.Should().Be(dto.Name);
        }

        [Fact]
        public async Task Create_Invalid_ReturnsBadRequest()
        {
            var dto = new ServiceCreateDto
            {
                Name = "",
                Price = -10,
                CategoryName = "Maintenance"
            };

            var response = await _client.PostAsJsonAsync("/api/Catalog/Service", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Update_Existing_ReturnsNoContent()
        {
            var dto = new ServiceCreateDto
            {
                Name = "Updated Service",
                Price = 300,
                CategoryName = "Maintenance"
            };

            var response = await _client.PutAsJsonAsync("/api/Catalog/Service/1", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Update_NotExisting_ReturnsNotFound()
        {
            var dto = new ServiceCreateDto
            {
                Name = "Test",
                Price = 100,
                CategoryName = "Maintenance"
            };

            var response = await _client.PutAsJsonAsync("/api/Catalog/Service/99999", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            var response = await _client.DeleteAsync("/api/Catalog/Service/1");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_NotExisting_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("/api/Catalog/Service/99999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Search_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/Catalog/Service/search?keyword=oil");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var data = await response.Content.ReadFromJsonAsync<List<ServiceDto>>();
            data.Should().NotBeNull();
        }

        [Fact]
        public async Task GetServicesAbovePrice_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/Catalog/Service/price/above/100");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var data = await response.Content.ReadFromJsonAsync<List<ServiceDto>>();
            data.Should().NotBeNull();
        }

        [Fact]
        public async Task GetServicesBelowPrice_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/Catalog/Service/price/below/1000");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var data = await response.Content.ReadFromJsonAsync<List<ServiceDto>>();
            data.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPagedServices_ReturnsOk()
        {
            var response = await _client.GetAsync(
                "/api/Catalog/Service/services?pageNumber=1&pageSize=5");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
