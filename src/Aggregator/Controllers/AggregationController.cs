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
            if (filter.Page is < 1 or > 100_000 || filter.PageSize is < 1 or > 100)
                return BadRequest("Invalid order pagination.");
            try
            {
                var result = await _service.GetAllOrdersWithReviewAsync(filter);
                if (filter.Page.HasValue)
                    return Ok(new { orders = result.Take(filter.PageSize), hasMore = result.Count > filter.PageSize });
                return Ok(result);
            }
            catch (Grpc.Core.RpcException ex) when (ex.StatusCode is Grpc.Core.StatusCode.Unavailable or Grpc.Core.StatusCode.DeadlineExceeded)
            {
                return StatusCode(503, new { message = "Один із сервісів замовлень тимчасово недоступний. Спробуйте ще раз." });
            }
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
