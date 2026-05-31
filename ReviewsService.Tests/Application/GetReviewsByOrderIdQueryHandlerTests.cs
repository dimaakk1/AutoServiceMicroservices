using Application.Cache;
using Application.DTO;
using Application.Handlers;
using Application.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;
using ReviewsService.Tests.Common;

namespace ReviewsService.Tests.Application;

public class GetReviewsByOrderIdQueryHandlerTests
{
    private readonly Mock<IReviewRepository> _repository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly TwoLevelCacheService<IEnumerable<ReviewDto>> _cache =
        TestCacheFactory.Create<IEnumerable<ReviewDto>>();
    private readonly GetReviewsByOrderIdQueryHandler _sut;

    public GetReviewsByOrderIdQueryHandlerTests()
    {
        _sut = new GetReviewsByOrderIdQueryHandler(_repository.Object, _mapper.Object, _cache);
    }

    [Fact]
    public async Task Handle_ReturnsMappedReviewsFromRepository()
    {
        const int orderId = 7;
        var reviews = new List<Review>
        {
            new(orderId, new Rating(5), "Excellent"),
            new(orderId, new Rating(4), "Good")
        };
        var dtos = new List<ReviewDto>
        {
            new() { Id = "1", OrderId = orderId, Rating = 5, Comment = "Excellent" },
            new() { Id = "2", OrderId = orderId, Rating = 4, Comment = "Good" }
        };

        _repository.Setup(r => r.GetByOrderIdAsync(orderId)).ReturnsAsync(reviews);
        _mapper.Setup(m => m.Map<IEnumerable<ReviewDto>>(reviews)).Returns(dtos);

        var result = (await _sut.Handle(new GetReviewsByOrderIdQuery { OrderId = orderId }, CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Rating == 5);
    }

    [Fact]
    public async Task Handle_NoReviews_ReturnsEmptyList()
    {
        const int orderId = 99;
        _repository.Setup(r => r.GetByOrderIdAsync(orderId)).ReturnsAsync([]);
        _mapper.Setup(m => m.Map<IEnumerable<ReviewDto>>(It.IsAny<IEnumerable<Review>>()))
            .Returns([]);

        var result = await _sut.Handle(new GetReviewsByOrderIdQuery { OrderId = orderId }, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_SecondCall_UsesCacheWithoutSecondRepositoryHit()
    {
        const int orderId = 3;
        var reviews = new List<Review> { new(orderId, new Rating(5), "Cached") };
        var dtos = new List<ReviewDto> { new() { Id = "1", OrderId = orderId, Rating = 5, Comment = "Cached" } };

        _repository.Setup(r => r.GetByOrderIdAsync(orderId)).ReturnsAsync(reviews);
        _mapper.Setup(m => m.Map<IEnumerable<ReviewDto>>(reviews)).Returns(dtos);

        var query = new GetReviewsByOrderIdQuery { OrderId = orderId };
        await _sut.Handle(query, CancellationToken.None);
        await _sut.Handle(query, CancellationToken.None);

        _repository.Verify(r => r.GetByOrderIdAsync(orderId), Times.Once);
    }
}
