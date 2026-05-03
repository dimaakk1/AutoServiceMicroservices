using AutoserviceCatalog.Tests.IntegrationTests.Infrastructure;
using AutoServiceCatalog.BLL.DTO;
using AutoServiceCatalog.DAL.Entities;
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
    public class ServiceDetailControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ServiceDetailControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/Catalog/ServiceDetail");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<List<ServiceDetailDto>>();
            body.Should().NotBeNull();
        }

        [Fact]
        public async Task GetById_Existing_ReturnsOk()
        {
            var all = await _client.GetFromJsonAsync<List<ServiceDetailDto>>(
                "/api/Catalog/ServiceDetail");

            var id = all!.First().ServiceDetailId;

            var response = await _client.GetAsync($"/api/Catalog/ServiceDetail/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<ServiceDetailDto>();
            body!.ServiceDetailId.Should().Be(id);
        }

        [Fact]
        public async Task GetById_NotExisting_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/Catalog/ServiceDetail/999999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_Valid_ReturnsCreated()
        {
            var serviceDto = new ServiceCreateDto
            {
                Name = "Test service unique " + Guid.NewGuid(),
                Price = 100,
                CategoryId = 1
            };

            var serviceResponse = await _client.PostAsJsonAsync(
                "/api/Catalog/Service", serviceDto);

            serviceResponse.EnsureSuccessStatusCode();

            var createdService = await serviceResponse.Content.ReadFromJsonAsync<ServiceDto>();

            var dto = new ServiceDetailCreateDto
            {
                ServiceId = createdService!.ServiceId,
                Manufacturer = "Bosch",
                Warranty = "12 months"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Catalog/ServiceDetail", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Update_Existing_ReturnsNoContent()
        {
            var all = await _client.GetFromJsonAsync<List<ServiceDetailDto>>(
                "/api/Catalog/ServiceDetail");

            var id = all!.First().ServiceDetailId;

            var dto = new ServiceDetailCreateDto
            {
                ServiceId = 1,
                Manufacturer = "Updated",
                Warranty = "24 months"
            };

            var response = await _client.PutAsJsonAsync(
                $"/api/Catalog/ServiceDetail/{id}", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            var all = await _client.GetFromJsonAsync<List<ServiceDetailDto>>(
                "/api/Catalog/ServiceDetail");

            var id = all!.Last().ServiceDetailId;

            var response = await _client.DeleteAsync(
                $"/api/Catalog/ServiceDetail/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task GetByManufacturer_ReturnsOk()
        {
            var response = await _client.GetAsync(
                "/api/Catalog/ServiceDetail/by-manufacturer/Bosch");

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.NotFound);
        }
    }
}
