using AutoServiceUsers.DAL.Entities;
using AutoServiceUsers.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using UserBLL = AutoServiceUsers.BLL.Services.UserService;

namespace AutoServiceUsers.Tests.BLL;

public class UserServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager = UserManagerMockFactory.Create();
    private readonly UserBLL _sut;

    public UserServiceTests()
    {
        _sut = new UserBLL(_userManager.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsDto()
    {
        const string userId = "user-123";
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com",
            LockoutEnd = null
        };

        _userManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(userId);

        result.UserId.Should().Be(userId);
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
        result.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_Throws()
    {
        _userManager.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);

        var act = () => _sut.GetByIdAsync("missing");

        await act.Should().ThrowAsync<Exception>().WithMessage("User not found");
    }

    [Fact]
    public async Task GetByIdAsync_BlockedUser_ReturnsIsBlockedTrue()
    {
        const string userId = "blocked";
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "blocked",
            Email = "blocked@test.com",
            LockoutEnd = DateTimeOffset.UtcNow.AddHours(2)
        };

        _userManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(userId);

        result.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedUsers()
    {
        var users = new List<ApplicationUser>
        {
            new() { Id = "1", UserName = "user1", Email = "u1@test.com", LockoutEnd = null },
            new()
            {
                Id = "2",
                UserName = "user2",
                Email = "u2@test.com",
                LockoutEnd = DateTimeOffset.UtcNow.AddHours(1)
            }
        }.AsQueryable();

        _userManager.Setup(m => m.Users).Returns(users);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(u => u.UserId == "1" && !u.IsBlocked);
        result.Should().Contain(u => u.UserId == "2" && u.IsBlocked);
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_DeletesSuccessfully()
    {
        const string userId = "del-1";
        var user = new ApplicationUser { Id = userId, UserName = "del", Email = "del@test.com" };

        _userManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        await _sut.DeleteAsync(userId);

        _userManager.Verify(m => m.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_Throws()
    {
        _userManager.Setup(m => m.FindByIdAsync("x")).ReturnsAsync((ApplicationUser?)null);

        var act = () => _sut.DeleteAsync("x");

        await act.Should().ThrowAsync<Exception>().WithMessage("User not found");
    }

    [Fact]
    public async Task DeleteAsync_IdentityFailure_Throws()
    {
        var user = new ApplicationUser { Id = "f", UserName = "f" };
        _userManager.Setup(m => m.FindByIdAsync("f")).ReturnsAsync(user);
        _userManager.Setup(m => m.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Cannot delete" }));

        var act = () => _sut.DeleteAsync("f");

        await act.Should().ThrowAsync<Exception>().WithMessage("Failed to delete user");
    }

    [Fact]
    public async Task BlockUserAsync_ExistingUser_SetsLockout()
    {
        const string userId = "block-1";
        var user = new ApplicationUser { Id = userId, UserName = "blockme" };

        _userManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        await _sut.BlockUserAsync(userId);

        user.LockoutEnabled.Should().BeTrue();
        user.LockoutEnd.Should().NotBeNull();
        user.LockoutEnd.Should().BeAfter(DateTimeOffset.UtcNow.AddYears(50));
        _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task BlockUserAsync_NotFound_Throws()
    {
        _userManager.Setup(m => m.FindByIdAsync("nope")).ReturnsAsync((ApplicationUser?)null);

        var act = () => _sut.BlockUserAsync("nope");

        await act.Should().ThrowAsync<Exception>().WithMessage("User not found");
    }

    [Fact]
    public async Task UnblockUserAsync_ExistingUser_ClearsLockout()
    {
        const string userId = "unblock-1";
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "unblock",
            LockoutEnd = DateTimeOffset.UtcNow.AddYears(1)
        };

        _userManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        await _sut.UnblockUserAsync(userId);

        user.LockoutEnd.Should().BeNull();
        _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UnblockUserAsync_NotFound_Throws()
    {
        _userManager.Setup(m => m.FindByIdAsync("nope")).ReturnsAsync((ApplicationUser?)null);

        var act = () => _sut.UnblockUserAsync("nope");

        await act.Should().ThrowAsync<Exception>().WithMessage("User not found");
    }
}
