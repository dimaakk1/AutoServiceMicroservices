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
    public class PartSupplierController : ControllerBase
    {
        private readonly IPartSupplierService _service;

        public PartSupplierController(IPartSupplierService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var links = await _service.GetAllAsync();
            return Ok(links);
        }

        [HttpGet("{partId}/{supplierId}")]
        public async Task<IActionResult> GetByIds(int partId, int supplierId)
        {
            var link = await _service.GetByIdsAsync(partId, supplierId);
            if (link == null)
                return NotFound();

            return Ok(link);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PartSupplierDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByIds), new { partId = created.PartId, supplierId = created.SupplierId }, created);
        }

        [HttpDelete("{partId}/{supplierId}")]
        public async Task<IActionResult> Delete(int partId, int supplierId)
        {
            try
            {
                await _service.DeleteAsync(partId, supplierId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpGet("part/{partId}/suppliers")]
        public async Task<IActionResult> GetSuppliersByPart(int partId)
        {
            var suppliers = await _service.GetSuppliersByPartIdAsync(partId);
            return Ok(suppliers);
        }

        [HttpGet("supplier/{supplierId}/parts")]
        public async Task<IActionResult> GetPartsBySupplier(int supplierId)
        {
            var parts = await _service.GetPartsBySupplierIdAsync(supplierId);
            return Ok(parts);
        }

    }
}
