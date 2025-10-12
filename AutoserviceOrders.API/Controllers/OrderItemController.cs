using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services;
using AutoserviceOrders.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoserviceOrders.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemController(IOrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var details = await _orderItemService.GetAllAsync();
            return Ok(details);
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetItemsByOrderId(int orderId)
        {
            var items = await _orderItemService.GetItemsByOrderIdAsync(orderId);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrderItem([FromBody] OrderItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _orderItemService.AddOrderItemAsync(dto);
            return Ok("Order item successfully added.");
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOrderItem([FromBody] OrderItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _orderItemService.UpdateOrderItemAsync(dto);
            return Ok("Order item successfully updated.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            await _orderItemService.DeleteOrderItemAsync(id);
            return Ok("Order item successfully deleted.");
        }
    }
}
