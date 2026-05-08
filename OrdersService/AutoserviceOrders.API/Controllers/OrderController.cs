using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services.Interfaces;
using AutoserviceOrders.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoserviceOrders.API.Controllers
{
    [Authorize]

    [Route("api/Orders/[controller]")]
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

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            orderDto.UserId = userId; // 🔥 додаємо

            var id = await _orderService.CreateOrderAsync(orderDto);

            orderDto.OrderId = id;

            return CreatedAtAction(nameof(GetById), new { id }, orderDto);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var orders = await _orderService.GetMyOrdersAsync(userId);

            return Ok(orders);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null || order.UserId != userId)
                return NotFound();

            order.Status = "Cancelled";

            await _orderService.UpdateOrderAsync(order);

            return Ok(new { message = "Замовлення скасовано" });
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
