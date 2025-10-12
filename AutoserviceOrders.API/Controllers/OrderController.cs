using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services.Interfaces;
using AutoserviceOrders.DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoserviceOrders.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderDto orderDto)
        {
            if (orderDto == null)
                return BadRequest();

            await _orderService.CreateOrderAsync(orderDto);
            return CreatedAtAction(nameof(GetById), new { id = orderDto.OrderId }, orderDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            return Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
            return Ok(orders);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] OrderDto orderDto)
        {
            if (orderDto == null)
                return BadRequest();

            bool updated = await _orderService.UpdateOrderAsync(orderDto);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _orderService.DeleteOrderAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            bool confirmed = await _orderService.ConfirmOrderAsync(id);
            return confirmed ? Ok(new { Message = "Order confirmed" }) : NotFound();
        }
    }
}
