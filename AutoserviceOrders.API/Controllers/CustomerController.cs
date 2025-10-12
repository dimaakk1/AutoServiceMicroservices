using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services.Interfaces;
using AutoserviceOrders.DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoserviceOrders.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound();

            return Ok(customer);
        }

        [HttpGet("email")]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
        {
            var customer = await _customerService.GetCustomerByEmailAsync(email);
            if (customer == null)
                return NotFound();

            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerDto customerDto)
        {
            if (customerDto == null)
                return BadRequest();

            await _customerService.AddCustomerAsync(customerDto);
            return CreatedAtAction(nameof(GetById), new { id = customerDto.CustomerId }, customerDto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerDto customerDto)
        {
            if (customerDto == null || customerDto.CustomerId != id)
                return BadRequest();

            await _customerService.UpdateCustomerAsync(customerDto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _customerService.DeleteCustomerAsync(id);
            return NoContent();
        }
    }
}
