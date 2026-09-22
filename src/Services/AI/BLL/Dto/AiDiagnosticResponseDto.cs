namespace AutoserviceAI.BLL.Dto;

public class AiDiagnosticResponseDto
{
    public string Reply { get; set; } = string.Empty;
    public AiServiceRecommendationDto? Recommendation { get; set; }
}

public class AiServiceRecommendationDto
{
    public int ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Reason { get; set; } = string.Empty;
}

internal class GeminiDiagnosticResultDto
{
    public string Reply { get; set; } = string.Empty;
    public int? RecommendedServiceId { get; set; }
    public string? RecommendationReason { get; set; }
}

internal class CatalogServiceDto
{
    public int ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
