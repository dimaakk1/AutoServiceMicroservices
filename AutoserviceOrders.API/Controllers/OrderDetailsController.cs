using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoserviceOrders.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly IOrderDetailsService _orderDetailsService;

        public OrderDetailsController(IOrderDetailsService orderDetailsService)
        {
            _orderDetailsService = orderDetailsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var details = await _orderDetailsService.GetAllAsync();
            return Ok(details);
        }

        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetByOrderId(int orderId)
        {
            var detail = await _orderDetailsService.GetByOrderIdAsync(orderId);
            if (detail == null)
                return NotFound();

            return Ok(detail);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderDetailsDto orderDetailsDto)
        {
            if (orderDetailsDto == null)
                return BadRequest();

            await _orderDetailsService.AddOrderDetailsAsync(orderDetailsDto);
            return CreatedAtAction(nameof(GetByOrderId), new { orderId = orderDetailsDto.OrderId }, orderDetailsDto);
        }

        [HttpPut("{orderId:int}")]
        public async Task<IActionResult> Update(int orderId, [FromBody] OrderDetailsDto orderDetailsDto)
        {
            if (orderDetailsDto == null || orderId != orderDetailsDto.OrderId)
                return BadRequest();

            await _orderDetailsService.UpdateOrderDetailsAsync(orderDetailsDto);
            return NoContent();
        }

        [HttpDelete("{orderId:int}")]
        public async Task<IActionResult> Delete(int orderId)
        {
            await _orderDetailsService.DeleteOrderDetailsAsync(orderId);
            return NoContent();
        }
    }
}
