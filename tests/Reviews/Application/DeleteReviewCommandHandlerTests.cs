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

public class DeleteReviewCommandHandlerTests
{
    private readonly Mock<IReviewRepository> _repository = new();
    private readonly TwoLevelCacheService<IEnumerable<ReviewDto>> _listCache =
        TestCacheFactory.Create<IEnumerable<ReviewDto>>();
    private readonly TwoLevelCacheService<ReviewDto> _orderCache = TestCacheFactory.Create<ReviewDto>();
    private readonly DeleteReviewCommandHandler _sut;

    public DeleteReviewCommandHandlerTests()
    {
        _sut = new DeleteReviewCommandHandler(_repository.Object, _listCache, _orderCache);
    }

    [Fact]
    public async Task Handle_ExistingReview_DeletesAndReturnsTrue()
    {
        const string reviewId = "review-1";
        var review = new Review(5, new Rating(4), "To delete");
        var command = new DeleteReviewCommand(reviewId);

        _repository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
        _repository.Setup(r => r.DeleteAsync(reviewId)).ReturnsAsync(true);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync(reviewId), Times.Once);
    }

    [Fact]
    public async Task Handle_ReviewNotFound_ReturnsFalse()
    {
        const string reviewId = "missing";
        _repository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((Review?)null);

        var result = await _sut.Handle(new DeleteReviewCommand(reviewId), CancellationToken.None);

        result.Should().BeFalse();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryDeleteFails_ReturnsFalse()
    {
        const string reviewId = "review-1";
        var review = new Review(2, new Rating(3), "Comment");
        _repository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
        _repository.Setup(r => r.DeleteAsync(reviewId)).ReturnsAsync(false);

        var result = await _sut.Handle(new DeleteReviewCommand(reviewId), CancellationToken.None);

        result.Should().BeFalse();
    }
}
