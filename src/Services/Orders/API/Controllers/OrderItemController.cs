using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services;
using AutoserviceOrders.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoserviceOrders.API.Controllers
{
    [Authorize]

    [Route("api/Orders/[controller]")]
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

        [HttpGet("with-items")]
        public async Task<IActionResult> GetWithItems()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                return Ok(await _orderItemService.GetOrdersWithItemsAsync(userId));
            }
            catch (Grpc.Core.RpcException ex) when (ex.StatusCode is Grpc.Core.StatusCode.Unavailable or Grpc.Core.StatusCode.DeadlineExceeded)
            {
                return StatusCode(503, new { message = "Каталог послуг тимчасово недоступний. Спробуйте ще раз." });
            }
        }

        [HttpGet("admin-with-items")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrdersWithItems(
        [FromQuery] OrderFilterDto filter)
        {
            var result = await _orderItemService.GetAllOrdersWithItemsAsync(filter);

            return Ok(result);
        }

        [HttpGet("mechanic-with-items")]
        [Authorize(Roles = "Mechanic")]
        public async Task<IActionResult> GetMechanicOrdersWithItems()
        {
            var mechanicId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(mechanicId))
                return Unauthorized();

            var result = await _orderItemService.GetAllOrdersWithItemsAsync(new OrderFilterDto
            {
                MechanicId = mechanicId
            });

            return Ok(result);
        }


    }
}
