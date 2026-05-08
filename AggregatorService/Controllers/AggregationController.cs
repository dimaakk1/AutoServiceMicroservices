using AggregatorService.DTO;
using AggregatorService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AggregatorService.Controllers
{
    [ApiController]
    [Route("api/aggregation")]
    public class AggregationController : ControllerBase
    {
        private readonly IAggregationService _service;

        public AggregationController(IAggregationService service)
        {
            _service = service;
        }

        // ======================================================
        // GET SINGLE ORDER WITH REVIEW
        // ======================================================
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetFullOrder(int orderId)
        {
            var result = await _service.GetOrderWithReviewAsync(orderId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ======================================================
        // GET ALL ORDERS WITH REVIEW
        // ======================================================
        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderAggregationFilterRequest filter)
        {
            var result = await _service.GetAllOrdersWithReviewAsync(filter);
            return Ok(result);
        }
        [Authorize]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _service.GetMyOrdersWithReviewAsync(userId);

            return Ok(result);
        }

        [HttpGet("orderswith-reviews")]
        public async Task<IActionResult> GetOrdersWithReviewsOnly()
        {
            var result = await _service.GetOrdersWithReviewsOnlyAsync();

            return Ok(result);
        }
    }
}