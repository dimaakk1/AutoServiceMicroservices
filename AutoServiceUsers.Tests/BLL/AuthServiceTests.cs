using AutoServiceUsers.BLL.DTO;
using AutoServiceUsers.BLL.Services;
using AutoServiceUsers.BLL.Services.Interfaces;
using AutoServiceUsers.DAL.Entities;
using AutoServiceUsers.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AutoServiceUsers.Tests.BLL;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager = UserManagerMockFactory.Create();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly Mock<IEmailService> _email = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userManager.Object, _jwt.Object, _email.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidData_CreatesUserAndSendsEmail()
    {
        var dto = new RegisterDto
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "Password123!"
        };

        _userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("confirm-token");
        _email.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.RegisterAsync(dto);

        result.Should().NotBeNull();
        _userManager.Verify(m => m.CreateAsync(It.Is<ApplicationUser>(u =>
            u.UserName == dto.Username && u.Email == dto.Email && !u.EmailConfirmed), dto.Password), Times.Once);
        _email.Verify(e => e.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WeakPassword_ThrowsWithIdentityErrors()
    {
        var dto = new RegisterDto { Username = "u", Email = "u@test.com", Password = "weak" };
        _userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        var act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Password too weak*");
        _email.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        var dto = new LoginDto { Username = "testuser", Password = "Password123!" };
        var user = new ApplicationUser
        {
            Id = "user-1",
            UserName = dto.Username,
            Email = "test@example.com",
            EmailConfirmed = true,
            RefreshTokens = []
        };
        var refresh = new RefreshToken
        {
            Token = "refresh-abc",
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        _userManager.Setup(m => m.FindByNameAsync(dto.Username)).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["User"]);
        _jwt.Setup(j => j.GenerateAccessToken(user, It.IsAny<IList<string>>())).Returns("access-token");
        _jwt.Setup(j => j.GenerateRefreshToken(user.Id)).Returns(refresh);
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.LoginAsync(dto);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-abc");
        user.RefreshTokens.Should().ContainSingle().Which.Token.Should().Be("refresh-abc");
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_ThrowsInvalidCredentials()
    {
        _userManager.Setup(m => m.FindByNameAsync("ghost")).ReturnsAsync((ApplicationUser?)null);

        var act = () => _sut.LoginAsync(new LoginDto { Username = "ghost", Password = "x" });

        await act.Should().ThrowAsync<Exception>().WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentials()
    {
        var user = new ApplicationUser { Id = "1", UserName = "u" };
        _userManager.Setup(m => m.FindByNameAsync("u")).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "bad")).ReturnsAsync(false);

        var act = () => _sut.LoginAsync(new LoginDto { Username = "u", Password = "bad" });

        await act.Should().ThrowAsync<Exception>().WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_UnconfirmedEmail_Throws()
    {
        var user = new ApplicationUser
        {
            Id = "1",
            UserName = "u",
            EmailConfirmed = false
        };
        _userManager.Setup(m => m.FindByNameAsync("u")).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "Password123!")).ReturnsAsync(true);

        var act = () => _sut.LoginAsync(new LoginDto { Username = "u", Password = "Password123!" });

        await act.Should().ThrowAsync<Exception>().WithMessage("Email not verified");
    }

    [Fact]
    public async Task LoginAsync_BlockedUser_Throws()
    {
        var user = new ApplicationUser
        {
            Id = "1",
            UserName = "u",
            EmailConfirmed = true,
            LockoutEnd = DateTimeOffset.UtcNow.AddHours(1)
        };
        _userManager.Setup(m => m.FindByNameAsync("u")).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "Password123!")).ReturnsAsync(true);

        var act = () => _sut.LoginAsync(new LoginDto { Username = "u", Password = "Password123!" });

        await act.Should().ThrowAsync<Exception>().WithMessage("User is blocked");
    }

    [Fact]
    public async Task VerifyEmailAsync_ValidToken_ConfirmsEmail()
    {
        var user = new ApplicationUser
        {
            Id = "uid",
            UserName = "u",
            Email = "u@test.com",
            EmailConfirmed = false
        };
        var dto = new VerifyEmailDto { UserId = user.Id, Token = "raw%2Btoken" };

        _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManager.Setup(m => m.ConfirmEmailAsync(user, "raw+token")).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        await _sut.VerifyEmailAsync(dto);

        user.EmailConfirmed.Should().BeTrue();
        _userManager.Verify(m => m.ConfirmEmailAsync(user, "raw+token"), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_AlreadyConfirmed_Throws()
    {
        var user = new ApplicationUser { Id = "uid", EmailConfirmed = true };
        _userManager.Setup(m => m.FindByIdAsync("uid")).ReturnsAsync(user);

        var act = () => _sut.VerifyEmailAsync(new VerifyEmailDto { UserId = "uid", Token = "t" });

        await act.Should().ThrowAsync<Exception>().WithMessage("Email already confirmed");
    }

    [Fact]
    public async Task VerifyEmailAsync_InvalidToken_Throws()
    {
        var user = new ApplicationUser { Id = "uid", EmailConfirmed = false };
        _userManager.Setup(m => m.FindByIdAsync("uid")).ReturnsAsync(user);
        _userManager.Setup(m => m.ConfirmEmailAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());

        var act = () => _sut.VerifyEmailAsync(new VerifyEmailDto { UserId = "uid", Token = "bad" });

        await act.Should().ThrowAsync<Exception>().WithMessage("Invalid or expired token");
    }
}

public class AuthServiceRefreshTokenTests
{
    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokens()
    {
        await using var fixture = await IdentityTestFixture.CreateAsync();
        var refresh = new RefreshToken
        {
            Token = "old-refresh",
            Expires = DateTime.UtcNow.AddDays(1),
            Created = DateTime.UtcNow
        };
        var user = await fixture.SeedUserAsync(refreshTokens: [refresh]);

        var jwt = new Mock<IJwtTokenService>();
        jwt.Setup(j => j.GenerateAccessToken(user, It.IsAny<IList<string>>())).Returns("new-access");
        jwt.Setup(j => j.GenerateRefreshToken(user.Id)).Returns(new RefreshToken
        {
            Token = "new-refresh",
            Expires = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        });

        var sut = new AuthService(fixture.UserManager, jwt.Object, Mock.Of<IEmailService>());

        var result = await sut.RefreshTokenAsync("old-refresh");

        result.AccessToken.Should().Be("new-access");
        result.RefreshToken.Should().Be("new-refresh");
    }

    [Fact]
    public async Task RefreshTokenAsync_UnknownToken_Throws()
    {
        await using var fixture = await IdentityTestFixture.CreateAsync();
        await fixture.SeedUserAsync();
        var sut = new AuthService(fixture.UserManager, Mock.Of<IJwtTokenService>(), Mock.Of<IEmailService>());

        var act = () => sut.RefreshTokenAsync("missing");

        await act.Should().ThrowAsync<Exception>().WithMessage("Refresh token not found");
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredToken_Throws()
    {
        await using var fixture = await IdentityTestFixture.CreateAsync();
        var expired = new RefreshToken
        {
            Token = "expired",
            Expires = DateTime.UtcNow.AddMinutes(-5),
            Created = DateTime.UtcNow.AddDays(-1)
        };
        await fixture.SeedUserAsync(refreshTokens: [expired]);
        var sut = new AuthService(fixture.UserManager, Mock.Of<IJwtTokenService>(), Mock.Of<IEmailService>());

        var act = () => sut.RefreshTokenAsync("expired");

        await act.Should().ThrowAsync<Exception>().WithMessage("Refresh token invalid");
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ValidToken_RevokesToken()
    {
        await using var fixture = await IdentityTestFixture.CreateAsync();
        var refresh = new RefreshToken
        {
            Token = "revoke-me",
            Expires = DateTime.UtcNow.AddDays(1),
            Created = DateTime.UtcNow
        };
        await fixture.SeedUserAsync(refreshTokens: [refresh]);
        var sut = new AuthService(fixture.UserManager, Mock.Of<IJwtTokenService>(), Mock.Of<IEmailService>());

        await sut.RevokeRefreshTokenAsync("revoke-me");

        var stored = await fixture.Context.RefreshTokens
            .FirstAsync(t => t.Token == "revoke-me");
        stored.Revoked.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_UnknownToken_Throws()
    {
        await using var fixture = await IdentityTestFixture.CreateAsync();
        var sut = new AuthService(fixture.UserManager, Mock.Of<IJwtTokenService>(), Mock.Of<IEmailService>());

        var act = () => sut.RevokeRefreshTokenAsync("nope");

        await act.Should().ThrowAsync<Exception>().WithMessage("Refresh token not found");
    }
}
