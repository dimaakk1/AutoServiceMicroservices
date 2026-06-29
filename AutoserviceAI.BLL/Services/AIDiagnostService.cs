using AutoserviceAI.BLL.Dto;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
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

        public async Task<string> ChatAsync(
            List<ChatMessageDto> messages)
        {
            var apiKey = Environment.GetEnvironmentVariable(
    "GEMINI_API_KEY");

            

            var prompt =
                """
            Ти професійний автомеханік.

            Допомагай визначати можливі
            несправності автомобіля.

            Якщо інформації недостатньо —
            став уточнюючі питання.

            Не став остаточний діагноз.
            Рекомендуй звернення до сервісу.
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
                                prompt +
                                "\n\n" +
                                conversation
                        }
                    }
                }
            }
            };

            var response =
                await _httpClient.PostAsJsonAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}",
                    requestBody);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content
                    .ReadFromJsonAsync<GeminiResponse>();

            return result?
                .Candidates?
                .FirstOrDefault()?
                .Content?
                .Parts?
                .FirstOrDefault()?
                .Text
                ?? "Не вдалося отримати відповідь.";
        }
    }
}
