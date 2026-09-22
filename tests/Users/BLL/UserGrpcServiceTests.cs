using AutoServiceUsers.BLL.DTO;
using AutoServiceUsers.BLL.Grpc;
using AutoServiceUsers.BLL.Services.Interfaces;
using FluentAssertions;
using Grpc.Core;
using Moq;
using Xunit;

namespace AutoServiceUsers.Tests.BLL;

public class UserGrpcServiceTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly UserGrpcService _sut;

    public UserGrpcServiceTests()
    {
        _sut = new UserGrpcService(_userService.Object);
    }

    [Fact]
    public async Task GetUser_ExistingUser_ReturnsGrpcResponse()
    {
        const string userId = "grpc-user";
        _userService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(new UserDto
        {
            UserId = userId,
            Username = "grpc",
            Email = "grpc@test.com"
        });

        var result = await _sut.GetUser(new UserRequest { UserId = userId }, null!);

        result.UserId.Should().Be(userId);
        result.Username.Should().Be("grpc");
        result.Email.Should().Be("grpc@test.com");
    }

    [Fact]
    public async Task GetUser_NotFound_PropagatesException()
    {
        _userService.Setup(s => s.GetByIdAsync("missing"))
            .ThrowsAsync(new Exception("User not found"));

        var act = () => _sut.GetUser(new UserRequest { UserId = "missing" }, null!);

        await act.Should().ThrowAsync<Exception>().WithMessage("User not found");
    }
}
