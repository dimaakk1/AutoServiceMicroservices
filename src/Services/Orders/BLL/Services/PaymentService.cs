using AutoMapper;
using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services.Interfaces;
using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.DAL.Repositories.Interfaces;
using AutoserviceOrders.DAL.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.BLL.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto)
        {
            var order = await _unitOfWork.GetRepository<IOrderRepository>().GetByIdAsync(createPaymentDto.OrderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order with ID {createPaymentDto.OrderId} not found");
            }

            var payment = new Payment
            {
                OrderId = createPaymentDto.OrderId,
                Amount = createPaymentDto.Amount,
                Currency = "UAH",
                Status = "Pending",
                PaymentMethod = "LiqPay",
                CreatedAt = DateTime.UtcNow,
                Description = createPaymentDto.Description ?? $"Payment for Order #{createPaymentDto.OrderId}"
            };

            var paymentRepository = _unitOfWork.GetRepository<IPaymentRepository>();
            var paymentId = await paymentRepository.AddAsync(payment);
            payment.PaymentId = paymentId;

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int paymentId)
        {
            var paymentRepository = _unitOfWork.GetRepository<IPaymentRepository>();
            var payment = await paymentRepository.GetByIdAsync(paymentId);

            return payment == null ? null : _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto?> GetPaymentByOrderIdAsync(int orderId)
        {
            var paymentRepository = _unitOfWork.GetRepository<IPaymentRepository>();
            var payment = await paymentRepository.GetPaymentByOrderIdAsync(orderId);

            return payment == null ? null : _mapper.Map<PaymentDto>(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByUserIdAsync(string userId)
        {
            var paymentRepository = _unitOfWork.GetRepository<IPaymentRepository>();
            var payments = await paymentRepository.GetPaymentsByUserIdAsync(userId);

            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<PaymentDto> UpdatePaymentStatusAsync(int paymentId, string status, string? transactionId)
        {
            var paymentRepository = _unitOfWork.GetRepository<IPaymentRepository>();
            var payment = await paymentRepository.GetByIdAsync(paymentId);

            if (payment == null)
            {
                throw new InvalidOperationException($"Payment with ID {paymentId} not found");
            }

            payment.Status = status;
            if (!string.IsNullOrEmpty(transactionId))
            {
                payment.TransactionId = transactionId;
            }

            if (status.ToLower() == "success" || status.ToLower() == "completed")
            {
                payment.PaidAt = DateTime.UtcNow;
                // Update order status
                var orderRepository = _unitOfWork.GetRepository<IOrderRepository>();
                var order = await orderRepository.GetByIdAsync(payment.OrderId);
                if (order != null && order.Status != "Cancelled")
                {
                    order.Status = "Confirmed";
                    await orderRepository.UpdateAsync(order);
                }
            }

            await paymentRepository.UpdateAsync(payment);

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            var paymentRepository = _unitOfWork.GetRepository<IPaymentRepository>();
            var payments = await paymentRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }
    }
}
