using AutoServiceUsers.BLL.Configuration;
using FluentAssertions;
using Xunit;

namespace AutoServiceUsers.Tests.BLL;

public class PublicUrlOptionsTests
{
    [Fact]
    public void BuildEmailVerificationUrl_EncodesValuesAndRemovesTrailingSlash()
    {
        var options = new PublicUrlOptions
        {
            ApiBaseUrl = "https://api.example.test/",
            FrontendBaseUrl = "https://app.example.test/"
        };

        var result = options.BuildEmailVerificationUrl("user id", "raw+token/=");

        result.Should().Be(
            "https://api.example.test/api/auth/verify-email?userId=user%20id&token=raw%2Btoken%2F%3D");
    }

    [Theory]
    [InlineData(true, "success")]
    [InlineData(false, "error")]
    public void BuildEmailConfirmationResultUrl_UsesConfiguredFrontend(bool success, string status)
    {
        var options = new PublicUrlOptions
        {
            FrontendBaseUrl = "https://app.example.test/"
        };

        options.BuildEmailConfirmationResultUrl(success)
            .Should().Be($"https://app.example.test/email-confirmed?status={status}");
    }
}
