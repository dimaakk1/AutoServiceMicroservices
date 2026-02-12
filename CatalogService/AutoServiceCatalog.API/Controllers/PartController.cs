using AutoServiceCatalog.BLL.DTO;
using AutoServiceCatalog.BLL.Services.Interfaces;
using AutoServiceCatalog.DAL.QueryParametrs;
using AutoServiceCatalog.DAL.Specefication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoServiceCatalog.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/Catalog/[controller]")]
    [ApiController]
    public class PartController : ControllerBase
    {
        private readonly IPartService _partService;

        public PartController(IPartService partService)
        {
            _partService = partService;
        }

        [HttpGet("parts")]
        public async Task<IActionResult> GetParts([FromQuery] PartQueryParameters parameters)
        {
            var parts = await _partService.GetPartsAsync(parameters);

            var result = parts.Items.Select(p => new PartDto
            {
                PartId = p.PartId,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.Category.CategoryId
            });

            return Ok(new PagedResult<PartDto>(result, parts.TotalCount, parameters.PageSize));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var parts = await _partService.GetAllAsync();
            return Ok(parts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var part = await _partService.GetByIdAsync(id);
                return Ok(part);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PartCreateDto dto)
        {
            try
            {
                var created = await _partService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.PartId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PartCreateDto dto)
        {
            try
            {
                await _partService.UpdateAsync(id, dto);
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
                await _partService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var parts = await _partService.SearchByNameAsync(keyword);
            return Ok(parts);
        }
        [HttpGet("price/above/{price}")]
        public async Task<IActionResult> GetPartsAbovePrice(decimal price)
        {
            var parts = await _partService.GetPartsAbovePriceAsync(price);
            if (parts == null || !parts.Any())
                return NotFound("No parts found above this price");

            return Ok(parts);
        }

        [HttpGet("price/below/{price}")]
        public async Task<IActionResult> GetPartsBelowPrice(decimal price)
        {
            var parts = await _partService.GetPartsBelowPriceAsync(price);
            if (parts == null || !parts.Any())
                return NotFound("No parts found below this price");

            return Ok(parts);
        }
    }

}
