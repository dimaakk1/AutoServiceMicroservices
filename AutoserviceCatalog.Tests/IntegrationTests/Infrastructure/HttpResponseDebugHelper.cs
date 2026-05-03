using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceCatalog.Tests.IntegrationTests.Infrastructure
{
    public static class HttpDebugHelper
    {
        public static async Task<string> Dump(HttpResponseMessage response)
        {
            var body = response.Content == null
                ? string.Empty
                : await response.Content.ReadAsStringAsync();

            var sb = new StringBuilder();

            sb.AppendLine("===== RESPONSE DEBUG =====");
            sb.AppendLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
            sb.AppendLine("--- BODY ---");
            sb.AppendLine(body);
            sb.AppendLine("==========================");

            return sb.ToString();
        }
    }
}
