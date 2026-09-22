namespace AutoServiceUsers.BLL.Configuration;

public sealed class PublicUrlOptions
{
    public const string SectionName = "PublicUrls";

    public string ApiBaseUrl { get; init; } = "http://localhost:5000";
    public string FrontendBaseUrl { get; init; } = "http://localhost:5173";

    public string BuildEmailVerificationUrl(string userId, string token)
    {
        return $"{Normalize(ApiBaseUrl)}/api/auth/verify-email" +
               $"?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
    }

    public string BuildEmailConfirmationResultUrl(bool success)
    {
        var status = success ? "success" : "error";
        return $"{Normalize(FrontendBaseUrl)}/email-confirmed?status={status}";
    }

    public void Validate()
    {
        ValidateHttpUrl(ApiBaseUrl, nameof(ApiBaseUrl));
        ValidateHttpUrl(FrontendBaseUrl, nameof(FrontendBaseUrl));
    }

    private static string Normalize(string value) => value.Trim().TrimEnd('/');

    private static void ValidateHttpUrl(string value, string name)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException($"{SectionName}:{name} must be an absolute HTTP or HTTPS URL.");
        }
    }
}
