using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services.Interfaces;
using AutoserviceOrders.BLL.Services.LiqPay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoserviceOrders.API.Controllers
{
    [Route("api/Orders/payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILiqPayService _liqPayService;
        private readonly IOrderService _orderService;

        public PaymentController(
            IPaymentService paymentService,
            ILiqPayService liqPayService,
            IOrderService orderService)
        {
            _paymentService = paymentService;
            _liqPayService = liqPayService;
            _orderService = orderService;
        }

        [HttpPost("checkout")]
        [Authorize]
        public async Task<IActionResult> Checkout([FromBody] CreatePaymentDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (dto == null || dto.OrderId <= 0)
                return BadRequest(new { message = "Invalid order id" });

            try
            {
                var order = await _orderService.GetOrderByIdAsync(dto.OrderId);

                if (order == null)
                    return NotFound(new { message = "Order not found" });

                if (order.UserId != userId)
                    return Forbid();

                var existingPayment =
                    await _paymentService.GetPaymentByOrderIdAsync(dto.OrderId);

                PaymentDto payment;

                if (existingPayment != null)
                {
                    payment = existingPayment;
                }
                else
                {
                    payment = await _paymentService.CreatePaymentAsync(dto);
                }

                var checkoutData = _liqPayService.GenerateCheckoutData(
     payment.OrderId,
     payment.Amount
 );

                return Ok(new
                {
                    paymentId = payment.PaymentId,
                    amount = payment.Amount,
                    data = checkoutData.Data,
                    signature = checkoutData.Signature
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

        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback(
            [FromForm] string data,
            [FromForm] string signature)
        {
            try
            {
                var callbackData =
                    await _liqPayService.ParseCallbackAsync(
                        data,
                        signature);

                await _paymentService.UpdatePaymentStatusAsync(
                    callbackData.PaymentId,
                    callbackData.Status,
                    callbackData.TransactionId);

                return Ok("ok");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LiqPay callback error: {ex.Message}");
                return BadRequest();
            }
        }

        [HttpGet("{paymentId}")]
        [Authorize]
        public async Task<IActionResult> GetPayment(int paymentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var payment =
                    await _paymentService.GetPaymentByIdAsync(paymentId);

                if (payment == null)
                    return NotFound();

                var order =
                    await _orderService.GetOrderByIdAsync(payment.OrderId);

                if (order == null || order.UserId != userId)
                    return Forbid();

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentByOrder(int orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var order =
                    await _orderService.GetOrderByIdAsync(orderId);

                if (order == null)
                    return NotFound();

                if (order.UserId != userId)
                    return Forbid();

                var payment =
                    await _paymentService.GetPaymentByOrderIdAsync(orderId);

                if (payment == null)
                    return NotFound();

                return Ok(payment);
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
        [Authorize]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var payments =
                    await _paymentService.GetPaymentsByUserIdAsync(userId);

                return Ok(payments);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPayments()
        {
            try
            {
                var payments =
                    await _paymentService.GetAllPaymentsAsync();

                return Ok(payments);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}