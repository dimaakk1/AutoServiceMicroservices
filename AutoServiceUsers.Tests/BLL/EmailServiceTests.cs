using AutoServiceUsers.BLL.Services;
using FluentAssertions;
using Xunit;

namespace AutoServiceUsers.Tests.BLL;

public class EmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_MissingCredentials_Throws()
    {
        var originalEmail = Environment.GetEnvironmentVariable("EMAIL");
        var originalPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

        try
        {
            Environment.SetEnvironmentVariable("EMAIL", null);
            Environment.SetEnvironmentVariable("EMAIL_PASSWORD", null);

            var sut = new EmailService();
            var act = () => sut.SendEmailAsync("to@test.com", "subject", "body");

            await act.Should().ThrowAsync<Exception>().WithMessage("Email credentials not found");
        }
        finally
        {
            Environment.SetEnvironmentVariable("EMAIL", originalEmail);
            Environment.SetEnvironmentVariable("EMAIL_PASSWORD", originalPassword);
        }
    }
}
