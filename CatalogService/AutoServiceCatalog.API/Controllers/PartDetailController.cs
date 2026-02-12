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
    public class PartDetailController : ControllerBase
    {
        private readonly IPartDetailService _service;

        public PartDetailController(IPartDetailService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var details = await _service.GetAllAsync();
            return Ok(details);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var detail = await _service.GetByIdAsync(id);
            if (detail == null)
                return NotFound();

            return Ok(detail);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PartDetailCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.PartDetailId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PartDetailCreateDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("by-manufacturer/{manufacturer}")]
        public async Task<IActionResult> GetByManufacturer(string manufacturer)
        {
            var result = await _service.GetByManufacturerAsync(manufacturer);
            if (result == null || result.Count == 0)
                return NotFound("No parts found for this manufacturer.");
            return Ok(result);
        }
    }
}
