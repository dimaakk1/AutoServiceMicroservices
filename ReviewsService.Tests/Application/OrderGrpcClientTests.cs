using Application.Grpc;
using FluentAssertions;
using ReviewsService.Tests.Common;

namespace ReviewsService.Tests.Application;

public class OrderGrpcClientTests
{
    [Fact]
    public async Task GetOrderAsync_ExistingOrder_ReturnsResponse()
    {
        var expected = new OrderResponse { OrderId = 10, UserId = "u1", Status = "Confirmed" };
        var client = new OrderGrpcClient(new FakeOrderServiceClient(new Dictionary<int, OrderResponse>
        {
            [10] = expected
        }));

        var result = await client.GetOrderAsync(10);

        result.Should().NotBeNull();
        result!.OrderId.Should().Be(10);
    }

    [Fact]
    public async Task GetOrderAsync_NotFound_ReturnsNull()
    {
        var client = new OrderGrpcClient(new FakeOrderServiceClient());

        var result = await client.GetOrderAsync(404);

        result.Should().BeNull();
    }
}
