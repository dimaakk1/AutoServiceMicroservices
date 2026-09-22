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

public class GetAllReviewsQueryHandlerTests
{
    private readonly Mock<IReviewRepository> _repository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly GetAllReviewsQueryHandler _sut;

    public GetAllReviewsQueryHandlerTests()
    {
        _sut = new GetAllReviewsQueryHandler(
            _repository.Object,
            _mapper.Object,
            TestCacheFactory.Create<IEnumerable<ReviewDto>>());
    }

    [Fact]
    public async Task Handle_ReturnsAllMappedReviews()
    {
        var reviews = new List<Review>
        {
            new(1, new Rating(5), "A"),
            new(2, new Rating(3), "B")
        };
        var dtos = new List<ReviewDto>
        {
            new() { Id = "1", OrderId = 1, Rating = 5, Comment = "A" },
            new() { Id = "2", OrderId = 2, Rating = 3, Comment = "B" }
        };

        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(reviews);
        _mapper.Setup(m => m.Map<IEnumerable<ReviewDto>>(reviews)).Returns(dtos);

        var result = (await _sut.Handle(new GetAllReviewsQuery(), CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
    }
}
