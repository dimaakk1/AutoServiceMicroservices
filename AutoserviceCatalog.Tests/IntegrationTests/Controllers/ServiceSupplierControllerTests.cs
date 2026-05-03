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
    public class ServiceSupplierControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ServiceSupplierControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/Catalog/ServiceSupplier");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<List<ServiceSupplierDto>>();
            body.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIds_Existing_ReturnsOk()
        {
            var all = await _client.GetFromJsonAsync<List<ServiceSupplierDto>>(
                "/api/Catalog/ServiceSupplier");

            var link = all!.First();

            var response = await _client.GetAsync(
                $"/api/Catalog/ServiceSupplier/{link.ServiceId}/{link.SupplierId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetByIds_NotExisting_ReturnsNotFound()
        {
            var response = await _client.GetAsync(
                "/api/Catalog/ServiceSupplier/99999/99999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_Valid_ReturnsCreated()
        {
            var category = new CategoryDto
            {
                Name = "Test Category"
            };

            var categoryResponse = await _client.PostAsJsonAsync(
                "/api/Catalog/Category", category);

            categoryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var createdCategory = await categoryResponse.Content
                .ReadFromJsonAsync<CategoryDto>();


            var serviceDto = new ServiceCreateDto
            {
                Name = "Test Service",
                Price = 100,
                CategoryId = createdCategory!.CategoryId
            };

            var serviceResponse = await _client.PostAsJsonAsync(
                "/api/Catalog/Service", serviceDto);

            serviceResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var createdService = await serviceResponse.Content
                .ReadFromJsonAsync<ServiceDto>();


            var supplierDto = new SupplierDto
            {
                Name = "Test Supplier",
                Phone = "123456789"
            };

            var supplierResponse = await _client.PostAsJsonAsync(
                "/api/Catalog/Supplier", supplierDto);

            supplierResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var createdSupplier = await supplierResponse.Content
                .ReadFromJsonAsync<SupplierDto>();


            var relationDto = new ServiceSupplierDto
            {
                ServiceId = createdService!.ServiceId,
                SupplierId = createdSupplier!.SupplierId
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Catalog/ServiceSupplier", relationDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var body = await response.Content.ReadFromJsonAsync<ServiceSupplierDto>();

            body!.ServiceId.Should().Be(relationDto.ServiceId);
            body.SupplierId.Should().Be(relationDto.SupplierId);
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            var all = await _client.GetFromJsonAsync<List<ServiceSupplierDto>>(
                "/api/Catalog/ServiceSupplier");

            var link = all!.First();

            var response = await _client.DeleteAsync(
                $"/api/Catalog/ServiceSupplier/{link.ServiceId}/{link.SupplierId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task GetSuppliersByService_ReturnsOk()
        {
            var services = await _client.GetFromJsonAsync<List<ServiceDto>>(
                "/api/Catalog/Service");

            var serviceId = services!.First().ServiceId;

            var response = await _client.GetAsync(
                $"/api/Catalog/ServiceSupplier/service/{serviceId}/suppliers");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetServicesBySupplier_ReturnsOk()
        {
            var suppliers = await _client.GetFromJsonAsync<List<SupplierDto>>(
                "/api/Catalog/Supplier");

            var supplierId = suppliers!.First().SupplierId;

            var response = await _client.GetAsync(
                $"/api/Catalog/ServiceSupplier/supplier/{supplierId}/services");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
