using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.BLL.DTO
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "UAH";
        public string Status { get; set; } = "Pending";
        public string PaymentMethod { get; set; } = "LiqPay";
        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? Description { get; set; }
    }

    public class CreatePaymentDto
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class LiqPayCallbackDto
    {
        public string Data { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }

    public class LiqPayResponseDto
    {
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? OrderId { get; set; }
        public decimal? Amount { get; set; }
    }
}
