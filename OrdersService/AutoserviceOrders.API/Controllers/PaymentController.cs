using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services.Interfaces;
using AutoserviceOrders.BLL.Services.LiqPay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoserviceOrders.API.Controllers
{
    [Route("api/Payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILiqPayService _liqPayService;
        private readonly IOrderService _orderService;

        public PaymentController(IPaymentService paymentService, ILiqPayService liqPayService, IOrderService orderService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _liqPayService = liqPayService ?? throw new ArgumentNullException(nameof(liqPayService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            if (createPaymentDto == null || createPaymentDto.OrderId <= 0)
                return BadRequest(new { message = "Invalid order ID" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            try
            {
                var order = await _orderService.GetOrderByIdAsync(createPaymentDto.OrderId);
                if (order == null || order.UserId != userId)
                    return Forbid();

                var payment = await _paymentService.CreatePaymentAsync(createPaymentDto);

                return Ok(new
                {
                    message = "Payment created successfully",
                    payment = payment
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("checkout")]
        [Authorize]
        public async Task<IActionResult> GetCheckoutForm([FromBody] CreatePaymentDto createPaymentDto)
        {
            if (createPaymentDto == null || createPaymentDto.OrderId <= 0)
                return BadRequest(new { message = "Invalid order ID" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            try
            {
                var order = await _orderService.GetOrderByIdAsync(createPaymentDto.OrderId);
                if (order == null || order.UserId != userId)
                    return Forbid();

                // Create payment record
                var payment = await _paymentService.CreatePaymentAsync(createPaymentDto);

                // Generate checkout form
                var checkoutForm = _liqPayService.GenerateCheckoutForm(
                    createPaymentDto.OrderId,
                    createPaymentDto.Amount,
                    userId
                );

                return Ok(new
                {
                    message = "Checkout form generated",
                    checkoutForm = checkoutForm,
                    payment = payment
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleCallback([FromBody] LiqPayCallbackDto callbackDto)
        {
            if (callbackDto == null || string.IsNullOrEmpty(callbackDto.Data) || string.IsNullOrEmpty(callbackDto.Signature))
                return BadRequest();

            try
            {
                var callbackData = await _liqPayService.ParseCallbackAsync(callbackDto.Data, callbackDto.Signature);

                // Update payment status
                var payment = await _paymentService.GetPaymentByOrderIdAsync(callbackData.OrderId);
                if (payment == null)
                    return BadRequest();

                var updatedPayment = await _paymentService.UpdatePaymentStatusAsync(
                    payment.PaymentId,
                    callbackData.Status,
                    callbackData.TransactionId
                );

                return Ok(new { message = "Payment processed successfully", payment = updatedPayment });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentByOrderId(int orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null || order.UserId != userId)
                    return Forbid();

                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                if (payment == null)
                    return NotFound();

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{paymentId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentById(int paymentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
                if (payment == null)
                    return NotFound();

                // Verify ownership
                var order = await _orderService.GetOrderByIdAsync(payment.OrderId);
                if (order == null || order.UserId != userId)
                    return Forbid();

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my/payments")]
        [Authorize]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            try
            {
                var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPayments()
        {
            try
            {
                var payments = await _paymentService.GetAllPaymentsAsync();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
