using AutoserviceAI.BLL.Dto;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoserviceAI.BLL.Services
{
    public class AiDiagnosticService : IAiDiagnosticService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiDiagnosticService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AiDiagnosticResponseDto> ChatAsync(
            List<ChatMessageDto> messages)
        {
            var apiKey = _configuration["GEMINI_API_KEY"]
                ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                ?? throw new InvalidOperationException("GEMINI_API_KEY is not configured.");
            var model = _configuration["GEMINI_MODEL"]
                ?? Environment.GetEnvironmentVariable("GEMINI_MODEL")
                ?? "gemini-2.5-flash";

            var availableServices = await GetAvailableServicesAsync();
            var servicesJson = JsonSerializer.Serialize(availableServices);

            var prompt =
                """
            Ти професійний автомеханік.

            Допомагай визначати можливі несправності автомобіля.

            Якщо інформації недостатньо — став уточнюючі питання.

            Відповідай ЛИШЕ валідним JSON без Markdown та без пояснень поза JSON:
            {
              "reply": "твоя відповідь українською",
              "recommendedServiceId": число або null,
              "recommendationReason": "коротке обґрунтування або null"
            }

            Обирай recommendedServiceId лише тоді, коли з опису вже можна обґрунтовано рекомендувати послугу.
            ID можна обрати тільки з переданого нижче каталогу. Не вигадуй послуг або ID.
            Якщо потрібні додаткові дані чи відповідної послуги немає — встанови recommendedServiceId у null.
            """;

            var conversation =
                string.Join("\n",
                    messages.Select(m =>
                        $"{m.Role}: {m.Content}"));

            var requestBody = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text =
                                prompt + "\n\nАктуальний каталог послуг:\n" + servicesJson +
                                "\n\nДіалог:\n" + conversation
                        }
                    }
                }
            }
            };

            var response =
                await _httpClient.PostAsJsonAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent?key={apiKey}",
                    requestBody);
            response.EnsureSuccessStatusCode();

            var result =
                await response.Content
                    .ReadFromJsonAsync<GeminiResponse>();

            var rawResult = result?
                .Candidates?
                .FirstOrDefault()?
                .Content?
                .Parts?
                .FirstOrDefault()?
                .Text
                ?? "Не вдалося отримати відповідь.";

            var diagnostic = TryParseDiagnosticResult(rawResult);
            if (diagnostic is null)
            {
                return new AiDiagnosticResponseDto { Reply = rawResult };
            }

            var recommendedService = diagnostic.RecommendedServiceId is int serviceId
                ? availableServices.FirstOrDefault(service => service.ServiceId == serviceId)
                : null;

            return new AiDiagnosticResponseDto
            {
                Reply = diagnostic.Reply,
                Recommendation = recommendedService is null
                    ? null
                    : new AiServiceRecommendationDto
                    {
                        ServiceId = recommendedService.ServiceId,
                        Name = recommendedService.Name,
                        Price = recommendedService.Price,
                        Reason = diagnostic.RecommendationReason ?? string.Empty
                    }
            };
        }

        private async Task<List<CatalogServiceDto>> GetAvailableServicesAsync()
        {
            var catalogUrl = _configuration["Services:CatalogUrl"]
                ?? "https://localhost:5001";

            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                return await _httpClient.GetFromJsonAsync<List<CatalogServiceDto>>(
                    $"{catalogUrl.TrimEnd('/')}/api/Catalog/Service",
                    timeout.Token) ?? [];
            }
            catch (HttpRequestException)
            {
                return [];
            }
            catch (TaskCanceledException)
            {
                return [];
            }
        }

        private static GeminiDiagnosticResultDto? TryParseDiagnosticResult(string value)
        {
            var json = value.Trim();
            if (json.StartsWith("```"))
            {
                json = json.Replace("```json", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("```", string.Empty)
                    .Trim();
            }

            try
            {
                return JsonSerializer.Deserialize<GeminiDiagnosticResultDto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
