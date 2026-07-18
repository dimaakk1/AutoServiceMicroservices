
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AutoserviceOrders.BLL.Services.LiqPay
{
    public interface ILiqPayService
    {
        LiqPayCheckoutDto GenerateCheckoutData(int paymentId, decimal amount);

        bool VerifyCallback(string data, string signature);

        Task<PaymentCallbackData> ParseCallbackAsync(string data, string signature);
    }

    public class LiqPayService : ILiqPayService
    {
        private readonly string _publicKey;
        private readonly string _privateKey;
        private readonly string _callbackUrl;
        private readonly string _resultUrl;

        public LiqPayService(IConfiguration configuration)
        {
            _publicKey = Environment.GetEnvironmentVariable("PublicKey")
                ?? throw new ArgumentNullException("LiqPay:PublicKey");

            _privateKey = Environment.GetEnvironmentVariable("PrivateKey")
                ?? throw new ArgumentNullException("LiqPay:PrivateKey");

            _callbackUrl = Environment.GetEnvironmentVariable("CallbackUrl")
                ?? throw new ArgumentNullException("LiqPay:CallbackUrl");

            _resultUrl = Environment.GetEnvironmentVariable("ResultUrl")
                ?? throw new ArgumentNullException("LiqPay:ResultUrl");
        }

        public LiqPayCheckoutDto GenerateCheckoutData(int paymentId, decimal amount)
        {
            var request = new
            {
                public_key = _publicKey,
                version = "3",
                action = "pay",
                amount = amount,
                currency = "UAH",
                description = $"Оплата замовлення #{paymentId}",
                order_id = paymentId.ToString(),
                server_url = _callbackUrl,
                result_url = _resultUrl,
                language = "uk"
            };

            var json = JsonSerializer.Serialize(request);

            var data = Base64Encode(json);

            var signature = GenerateSignature(data);

            return new LiqPayCheckoutDto
            {
                Data = data,
                Signature = signature
            };
        }

        public bool VerifyCallback(string data, string signature)
        {
            var expectedSignature = GenerateSignature(data);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signature),
                Encoding.UTF8.GetBytes(expectedSignature));
        }

        public async Task<PaymentCallbackData> ParseCallbackAsync(
            string data,
            string signature)
        {
            if (!VerifyCallback(data, signature))
            {
                throw new InvalidOperationException("Invalid LiqPay signature");
            }

            var decodedData = Base64Decode(data);

            using var document = JsonDocument.Parse(decodedData);

            var root = document.RootElement;

            return await Task.FromResult(new PaymentCallbackData
            {
                PaymentId = root.TryGetProperty("order_id", out var orderId)
                    ? int.Parse(orderId.GetString() ?? "0")
                    : 0,

                Amount = root.TryGetProperty("amount", out var amount)
                    ? amount.GetDecimal()
                    : 0,

                Status = root.TryGetProperty("status", out var status)
                    ? status.GetString() ?? "pending"
                    : "pending",

                TransactionId = root.TryGetProperty("transaction_id", out var txId)
                    ? txId.GetString()
                    : null,

                Description = root.TryGetProperty("description", out var desc)
                    ? desc.GetString()
                    : null
            });
        }

        private string GenerateSignature(string data)
        {
            var signatureString = $"{_privateKey}{data}{_privateKey}";

            using var sha1 = SHA1.Create();

            var hash = sha1.ComputeHash(
                Encoding.UTF8.GetBytes(signatureString));

            return Convert.ToBase64String(hash);
        }

        private static string Base64Encode(string text)
        {
            return Convert.ToBase64String(
                Encoding.UTF8.GetBytes(text));
        }

        private static string Base64Decode(string base64)
        {
            return Encoding.UTF8.GetString(
                Convert.FromBase64String(base64));
        }
    }

    public class LiqPayCheckoutDto
    {
        public string Data { get; set; } = string.Empty;

        public string Signature { get; set; } = string.Empty;
    }

    public class PaymentCallbackData
    {
        public int PaymentId { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? TransactionId { get; set; }

        public string? Description { get; set; }
    }
}
