using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services.Interfaces;
using AutoserviceOrders.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Grpc.Core;
using VehicleGrpc;
using GrpcStatusCode = Grpc.Core.StatusCode;

namespace AutoserviceOrders.API.Controllers
{
    [Authorize]

    [Route("api/Orders/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly VehicleRegistry.VehicleRegistryClient _vehicleClient;

        public OrderController(IOrderService orderService, VehicleRegistry.VehicleRegistryClient vehicleClient)
        {
            _orderService = orderService;
            _vehicleClient = vehicleClient;
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
            orderDto.OrderDate = DateTime.SpecifyKind(orderDto.OrderDate, DateTimeKind.Utc);
            try
            {
                if (orderDto.VehicleId.HasValue)
                {
                    var vehicle = await _vehicleClient.GetOwnedVehicleAsync(new GetOwnedVehicleRequest
                    {
                        VehicleId = orderDto.VehicleId.Value.ToString(),
                        UserId = userId
                    }, deadline: DateTime.UtcNow.AddSeconds(5), cancellationToken: HttpContext.RequestAborted);

                    orderDto.VehicleId = Guid.Parse(vehicle.VehicleId);
                    orderDto.VehicleDisplayName = $"{vehicle.Make} {vehicle.Model} ({vehicle.Year})";
                    orderDto.VehicleVin = vehicle.Vin;
                    orderDto.VehicleLicensePlate = string.IsNullOrWhiteSpace(vehicle.LicensePlate)
                        ? null
                        : vehicle.LicensePlate;
                }
                else
                {
                    orderDto.VehicleDisplayName = null;
                    orderDto.VehicleVin = null;
                    orderDto.VehicleLicensePlate = null;
                }

                var id = await _orderService.CreateOrderAsync(orderDto);

                orderDto.OrderId = id;

                return CreatedAtAction(nameof(GetById), new { id }, orderDto);
            }
            catch (RpcException ex) when (ex.StatusCode is GrpcStatusCode.NotFound or GrpcStatusCode.InvalidArgument)
            {
                return BadRequest(new { message = "Обраний автомобіль не знайдено у вашому гаражі." });
            }
            catch (RpcException ex) when (ex.StatusCode is GrpcStatusCode.Unavailable or GrpcStatusCode.DeadlineExceeded)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = "Не вдалося перевірити автомобіль. Спробуйте ще раз або створіть запис без автомобіля."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }

            
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

        [HttpGet("taken-slots")]
        public async Task<IActionResult> GetTakenSlots(DateTime date)
        {
            var slots = await _orderService.GetTakenSlotsAsync(date);

            return Ok(slots);
        }

        [HttpGet("by-date")]
        public async Task<IActionResult> GetByDate([FromQuery] DateTime date)
        {
            var result = await _orderService.GetOrdersByDateAsync(date);
            return Ok(result);
        }
    }
}
