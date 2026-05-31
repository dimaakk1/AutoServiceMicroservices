using Domain.ValueObjects;
using FluentAssertions;

namespace ReviewsService.Tests.Domain;

public class RatingTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Constructor_ValidValue_CreatesRating(int value)
    {
        var rating = new Rating(value);

        rating.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Constructor_InvalidValue_Throws(int value)
    {
        var act = () => new Rating(value);

        act.Should().Throw<ArgumentException>().WithMessage("*between 1 and 5*");
    }
}
