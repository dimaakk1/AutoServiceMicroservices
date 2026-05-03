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
    public class SupplierControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public SupplierControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/Catalog/Supplier");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<List<SupplierDto>>();
            body.Should().NotBeNull();
        }

        [Fact]
        public async Task GetById_Existing_ReturnsOk()
        {
            var suppliers = await _client.GetFromJsonAsync<List<SupplierDto>>(
                "/api/Catalog/Supplier");

            var id = suppliers!.First().SupplierId;

            var response = await _client.GetAsync($"/api/Catalog/Supplier/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<SupplierDto>();
            body!.SupplierId.Should().Be(id);
        }

        [Fact]
        public async Task GetById_NotExisting_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/Catalog/Supplier/999999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_Valid_ReturnsCreated()
        {
            var dto = new SupplierCreateDto
            {
                Name = "Test Supplier",
                Phone = "+380991112233"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Catalog/Supplier", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var body = await response.Content.ReadFromJsonAsync<SupplierDto>();
            body!.Name.Should().Be(dto.Name);
        }

        [Fact]
        public async Task Update_Existing_ReturnsNoContent()
        {
            var suppliers = await _client.GetFromJsonAsync<List<SupplierDto>>(
                "/api/Catalog/Supplier");

            var id = suppliers!.First().SupplierId;

            var dto = new SupplierCreateDto
            {
                Name = "Updated Supplier",
                Phone = "+380000000000"
            };

            var response = await _client.PutAsJsonAsync(
                $"/api/Catalog/Supplier/{id}", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            var suppliers = await _client.GetFromJsonAsync<List<SupplierDto>>(
                "/api/Catalog/Supplier");

            var id = suppliers!.Last().SupplierId;

            var response = await _client.DeleteAsync(
                $"/api/Catalog/Supplier/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Search_ReturnsOk()
        {
            var response = await _client.GetAsync(
                "/api/Catalog/Supplier/search?keyword=a");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<List<SupplierDto>>();
            body.Should().NotBeNull();
        }

        [Fact]
        public async Task GetSupplierWithServices_ReturnsOk()
        {
            var suppliers = await _client.GetFromJsonAsync<List<SupplierDto>>(
                "/api/Catalog/Supplier");

            var id = suppliers!.First().SupplierId;

            var response = await _client.GetAsync(
                $"/api/Catalog/Supplier/{id}/with-services");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
