using AutoserviceOrders.BLL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.BLL.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto);
        Task<PaymentDto?> GetPaymentByIdAsync(int paymentId);
        Task<PaymentDto?> GetPaymentByOrderIdAsync(int orderId);
        Task<IEnumerable<PaymentDto>> GetPaymentsByUserIdAsync(string userId);
        Task<PaymentDto> UpdatePaymentStatusAsync(int paymentId, string status, string? transactionId);
        Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();
    }
}
