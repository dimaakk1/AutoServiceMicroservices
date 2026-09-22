using AutoServiceCatalog.BLL.DTO;
using AutoServiceCatalog.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoServiceCatalog.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/Catalog/[controller]")]
    [ApiController]
    public class ServiceSupplierController : ControllerBase
    {
        private readonly IServiceSupplierService _service;

        public ServiceSupplierController(IServiceSupplierService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var links = await _service.GetAllAsync();
            return Ok(links);
        }

        [HttpGet("{serviceId}/{supplierId}")]
        public async Task<IActionResult> GetByIds(int serviceId, int supplierId)
        {
            var link = await _service.GetByIdsAsync(serviceId, supplierId);
            if (link == null)
                return NotFound();

            return Ok(link);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceSupplierDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByIds), new { serviceId = created.ServiceId, supplierId = created.SupplierId }, created);
        }

        [HttpDelete("{serviceId}/{supplierId}")]
        public async Task<IActionResult> Delete(int serviceId, int supplierId)
        {
            try
            {
                await _service.DeleteAsync(serviceId, supplierId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpGet("service/{serviceId}/suppliers")]
        public async Task<IActionResult> GetSuppliersByService(int serviceId)
        {
            var suppliers = await _service.GetSuppliersByServiceIdAsync(serviceId);
            return Ok(suppliers);
        }

        [HttpGet("supplier/{supplierId}/services")]
        public async Task<IActionResult> GetServicesBySupplier(int supplierId)
        {
            var services = await _service.GetServicesBySupplierIdAsync(supplierId);
            return Ok(services);
        }

    }
}
