using Application.DTO;
using Application.Grpc;
using Application.Queries;
using FluentAssertions;
using Grpc.Core;
using MediatR;
using Moq;

namespace ReviewsService.Tests.Application;

public class ReviewServiceImplTests
{
    [Fact]
    public async Task GetReviewsByOrderId_StreamsMappedResponses()
    {
        const int orderId = 5;
        var dtos = new List<ReviewDto>
        {
            new()
            {
                Id = "r1",
                OrderId = orderId,
                Rating = 5,
                Comment = "Great",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.Is<GetReviewsByOrderIdQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dtos);

        var sut = new ReviewServiceImpl(mediator.Object);
        var stream = new TestServerStreamWriter<ReviewResponse>();

        await sut.GetReviewsByOrderId(new ReviewRequest { OrderId = orderId }, stream, null!);

        stream.Messages.Should().ContainSingle();
        stream.Messages[0].Id.Should().Be("r1");
        stream.Messages[0].Rating.Should().Be(5);
        stream.Messages[0].Comment.Should().Be("Great");
    }

    private sealed class TestServerStreamWriter<T> : IServerStreamWriter<T> where T : class
    {
        public List<T> Messages { get; } = [];
        public WriteOptions? WriteOptions { get; set; }

        public Task WriteAsync(T message)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
