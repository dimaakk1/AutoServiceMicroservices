using AutoServiceUsers.BLL.Services;
using AutoServiceUsers.DAL.Entities;
using AutoServiceUsers.Tests.Common;
using FluentAssertions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace AutoServiceUsers.Tests.BLL;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut = new(JwtConfigurationFactory.Create());

    [Fact]
    public void GenerateAccessToken_IncludesUserClaimsAndRoles()
    {
        var user = new ApplicationUser
        {
            Id = "user-42",
            UserName = "john",
            Email = "john@test.com"
        };

        var token = _sut.GenerateAccessToken(user, ["User", "Admin"]);

        token.Should().NotBeNullOrWhiteSpace();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "user-42");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "john");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueTokenWithExpiry()
    {
        var token1 = _sut.GenerateRefreshToken("user-1");
        var token2 = _sut.GenerateRefreshToken("user-1");

        token1.Token.Should().NotBeNullOrWhiteSpace();
        token1.Token.Should().NotBe(token2.Token);
        token1.UserId.Should().Be("user-1");
        token1.Expires.Should().BeAfter(DateTime.UtcNow.AddDays(6));
    }

    [Fact]
    public void ValidateToken_ValidToken_ReturnsPrincipal()
    {
        var user = new ApplicationUser { Id = "1", UserName = "u" };
        var token = _sut.GenerateAccessToken(user, ["User"]);

        var principal = _sut.ValidateToken(token);

        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("1");
    }
}
