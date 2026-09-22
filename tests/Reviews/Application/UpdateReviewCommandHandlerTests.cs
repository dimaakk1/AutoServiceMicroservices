using Application.Cache;
using Application.Commands;
using Application.DTO;
using Application.Handlers;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;
using ReviewsService.Tests.Common;

namespace ReviewsService.Tests.Application;

public class UpdateReviewCommandHandlerTests
{
    private readonly Mock<IReviewRepository> _repository = new();
    private readonly TwoLevelCacheService<IEnumerable<ReviewDto>> _listCache =
        TestCacheFactory.Create<IEnumerable<ReviewDto>>();
    private readonly TwoLevelCacheService<ReviewDto> _orderCache = TestCacheFactory.Create<ReviewDto>();
    private readonly UpdateReviewCommandHandler _sut;

    public UpdateReviewCommandHandlerTests()
    {
        _sut = new UpdateReviewCommandHandler(_repository.Object, _listCache, _orderCache);
    }

    [Fact]
    public async Task Handle_ExistingReview_UpdatesAndReturnsTrue()
    {
        const string reviewId = "review-1";
        var existing = new Review(1, new Rating(5), "Old comment");
        var command = new UpdateReviewCommand
        {
            Id = reviewId,
            Rating = 4,
            Comment = "Updated comment"
        };

        _repository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(existing);
        _repository.Setup(r => r.UpdateAsync(existing)).Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        existing.Rating.Value.Should().Be(4);
        existing.Comment.Should().Be("Updated comment");
        _repository.Verify(r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task Handle_ReviewNotFound_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((Review?)null);

        var result = await _sut.Handle(new UpdateReviewCommand
        {
            Id = "missing",
            Rating = 3,
            Comment = "x"
        }, CancellationToken.None);

        result.Should().BeFalse();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Review>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidRating_ThrowsArgumentException()
    {
        var existing = new Review(1, new Rating(3), "Comment");
        _repository.Setup(r => r.GetByIdAsync("id")).ReturnsAsync(existing);

        var act = () => _sut.Handle(new UpdateReviewCommand
        {
            Id = "id",
            Rating = 0,
            Comment = "bad"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
