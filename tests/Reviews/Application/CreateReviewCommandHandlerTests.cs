using Application.Cache;
using Application.Commands;
using Application.DTO;
using Application.Grpc;
using Application.Handlers;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Grpc.Core;
using Moq;
using ReviewsService.Tests.Common;

namespace ReviewsService.Tests.Application;

public class CreateReviewCommandHandlerTests
{
    private readonly Mock<IReviewRepository> _repository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly TwoLevelCacheService<ReviewDto> _cache = TestCacheFactory.Create<ReviewDto>();

    private CreateReviewCommandHandler CreateSut(OrderGrpcClient orderClient) =>
        new(_repository.Object, _mapper.Object, orderClient, _cache);

    [Fact]
    public async Task Handle_ValidReview_PersistsAndInvalidatesCache()
    {
        var orders = new Dictionary<int, OrderResponse>
        {
            [1] = new() { OrderId = 1, UserId = "user-1", Status = "Completed" }
        };
        var sut = CreateSut(new OrderGrpcClient(new FakeOrderServiceClient(orders)));

        var command = new CreateReviewCommand
        {
            OrderId = 1,
            Rating = 5,
            Comment = "Great service!"
        };
        var dto = new ReviewDto
        {
            Id = "review-1",
            OrderId = 1,
            Rating = 5,
            Comment = "Great service!"
        };

        _repository.Setup(r => r.AddAsync(It.IsAny<Review>())).Returns(Task.CompletedTask);
        _mapper.Setup(m => m.Map<ReviewDto>(It.IsAny<Review>())).Returns(dto);

        var result = await sut.Handle(command, CancellationToken.None);

        result.Should().BeEquivalentTo(dto);
        _repository.Verify(r => r.AddAsync(It.Is<Review>(rev =>
            rev.OrderId == 1 && rev.Rating.Value == 5 && rev.Comment == "Great service!")), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsRpcException()
    {
        var sut = CreateSut(new OrderGrpcClient(new FakeOrderServiceClient()));
        var command = new CreateReviewCommand { OrderId = 999, Rating = 5, Comment = "Test" };

        var act = () => sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RpcException>()
            .Where(ex => ex.StatusCode == StatusCode.NotFound);
        _repository.Verify(r => r.AddAsync(It.IsAny<Review>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidRating_ThrowsArgumentException()
    {
        var orders = new Dictionary<int, OrderResponse> { [1] = new() { OrderId = 1 } };
        var sut = CreateSut(new OrderGrpcClient(new FakeOrderServiceClient(orders)));
        var command = new CreateReviewCommand { OrderId = 1, Rating = 6, Comment = "Test" };

        var act = () => sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_EmptyComment_ThrowsArgumentException()
    {
        var orders = new Dictionary<int, OrderResponse> { [1] = new() { OrderId = 1 } };
        var sut = CreateSut(new OrderGrpcClient(new FakeOrderServiceClient(orders)));
        var command = new CreateReviewCommand { OrderId = 1, Rating = 4, Comment = "   " };

        var act = () => sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
