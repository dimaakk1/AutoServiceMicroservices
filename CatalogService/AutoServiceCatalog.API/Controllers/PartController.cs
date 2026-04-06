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
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public ServiceController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetServices([FromQuery] PartQueryParameters parameters)
        {
            var services = await _serviceService.GetServicesAsync(parameters);

            var result = services.Items.Select(s => new ServiceDto
            {
                ServiceId = s.ServiceId,
                Name = s.Name,
                Price = s.Price,
                CategoryId = s.Category.CategoryId
            });

            return Ok(new PagedResult<ServiceDto>(result, services.TotalCount, parameters.PageSize));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _serviceService.GetAllAsync();
            return Ok(services);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var service = await _serviceService.GetByIdAsync(id);
                return Ok(service);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceCreateDto dto)
        {
            try
            {
                var created = await _serviceService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.ServiceId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceCreateDto dto)
        {
            try
            {
                await _serviceService.UpdateAsync(id, dto);
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
                await _serviceService.DeleteAsync(id);
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
            var services = await _serviceService.SearchByNameAsync(keyword);
            return Ok(services);
        }
        [HttpGet("price/above/{price}")]
        public async Task<IActionResult> GetServicesAbovePrice(decimal price)
        {
            var services = await _serviceService.GetServicesAbovePriceAsync(price);
            if (services == null || !services.Any())
                return NotFound("No services found above this price");

            return Ok(services);
        }

        [HttpGet("price/below/{price}")]
        public async Task<IActionResult> GetServicesBelowPrice(decimal price)
        {
            var services = await _serviceService.GetServicesBelowPriceAsync(price);
            if (services == null || !services.Any())
                return NotFound("No services found below this price");

            return Ok(services);
        }
    }

}
