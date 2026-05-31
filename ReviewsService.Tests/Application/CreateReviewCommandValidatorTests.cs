using Application.Commands;
using Application.Validators;
using FluentAssertions;
using Xunit;

namespace ReviewsService.Tests.Application;

public class CreateReviewCommandValidatorTests
{
    private readonly CreateReviewCommandValidator _sut = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = new CreateReviewCommand
        {
            OrderId = 1,
            Rating = 5,
            Comment = "Valid comment"
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidOrderId_Fails(int orderId)
    {
        var command = new CreateReviewCommand { OrderId = orderId, Rating = 5, Comment = "x" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateReviewCommand.OrderId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Validate_InvalidRating_Fails(int rating)
    {
        var command = new CreateReviewCommand { OrderId = 1, Rating = rating, Comment = "x" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateReviewCommand.Rating));
    }

    [Fact]
    public void Validate_EmptyComment_Fails()
    {
        var command = new CreateReviewCommand { OrderId = 1, Rating = 4, Comment = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateReviewCommand.Comment));
    }

    [Fact]
    public void Validate_TooLongComment_Fails()
    {
        var command = new CreateReviewCommand
        {
            OrderId = 1,
            Rating = 4,
            Comment = new string('a', 501)
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
