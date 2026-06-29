using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AutoserviceOrders.BLL.Services.LiqPay
{
    public interface ILiqPayService
    {
        string GenerateCheckoutForm(int orderId, decimal amount, string userId);
        bool VerifyCallback(string data, string signature);
        Task<PaymentCallbackData> ParseCallbackAsync(string data, string signature);
    }

    public class LiqPayService : ILiqPayService
    {
        private readonly string _publicKey;
        private readonly string _privateKey;
        private readonly string _callbackUrl;

        public LiqPayService(IConfiguration configuration)
        {
            _publicKey = configuration["LiqPay:PublicKey"] ?? throw new ArgumentNullException("LiqPay:PublicKey");
            _privateKey = configuration["LiqPay:PrivateKey"] ?? throw new ArgumentNullException("LiqPay:PrivateKey");
            _callbackUrl = configuration["LiqPay:CallbackUrl"] ?? "https://yourdomain.com/api/payments/callback";
        }

        public string GenerateCheckoutForm(int orderId, decimal amount, string userId)
        {
            var request = new
            {
                public_key = _publicKey,
                version = "3",
                action = "pay",
                amount = (int)(amount * 100), // конвертуємо в копійки
                currency = "UAH",
                description = $"Order #{orderId}",
                order_id = orderId.ToString(),
                server_url = _callbackUrl,
                result_url = "https://yourdomain.com/orders/success",
                language = "uk"
            };

            var data = Base64Encode(JsonSerializer.Serialize(request));
            var signature = GenerateSignature(data);

            return GenerateHtmlForm(data, signature);
        }

        public bool VerifyCallback(string data, string signature)
        {
            var expectedSignature = GenerateSignature(data);
            return signature == expectedSignature;
        }

        public async Task<PaymentCallbackData> ParseCallbackAsync(string data, string signature)
        {
            if (!VerifyCallback(data, signature))
            {
                throw new InvalidOperationException("Invalid callback signature");
            }

            var decodedData = Base64Decode(data);
            using var doc = JsonDocument.Parse(decodedData);
            var root = doc.RootElement;

            return new PaymentCallbackData
            {
                OrderId = root.TryGetProperty("order_id", out var orderId) ? int.Parse(orderId.GetString() ?? "0") : 0,
                Amount = root.TryGetProperty("amount", out var amount) ? amount.GetDecimal() / 100 : 0,
                Status = root.TryGetProperty("status", out var status) ? status.GetString() ?? "pending" : "pending",
                TransactionId = root.TryGetProperty("transaction_id", out var txId) ? txId.GetString() : null,
                Description = root.TryGetProperty("description", out var desc) ? desc.GetString() : null
            };
        }

        private string GenerateSignature(string data)
        {
            var str = _privateKey + data + _privateKey;
            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
            return Convert.ToBase64String(hash);
        }

        private string Base64Encode(string text)
        {
            var textBytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(textBytes);
        }

        private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private string GenerateHtmlForm(string data, string signature)
        {
            return $@"
<form method='POST' action='https://www.liqpay.com/api/3/checkout' accept-charset='utf-8'>
    <input type='hidden' name='data' value='{data}' />
    <input type='hidden' name='signature' value='{signature}' />
    <button type='submit'>Оплатити</button>
</form>";
        }
    }

    public class PaymentCallbackData
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? Description { get; set; }
    }
}
