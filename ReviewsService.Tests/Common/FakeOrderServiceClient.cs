using Grpc.Core;

namespace ReviewsService.Tests.Common;

internal sealed class FakeOrderServiceClient : OrderService.OrderServiceClient
{
    private readonly IReadOnlyDictionary<int, OrderResponse> _orders;

    public FakeOrderServiceClient(IReadOnlyDictionary<int, OrderResponse>? orders = null)
        : base(new StubCallInvoker())
    {
        _orders = orders ?? new Dictionary<int, OrderResponse>();
    }

    public override AsyncUnaryCall<OrderResponse> GetOrderAsync(OrderRequest request, CallOptions options)
    {
        if (!_orders.TryGetValue(request.OrderId, out var order))
        {
            return GrpcResponses.NotFound<OrderResponse>();
        }

        return GrpcResponses.Success(order);
    }

    private sealed class StubCallInvoker : CallInvoker
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options,
            TRequest request) =>
            throw new NotSupportedException("Use FakeOrderServiceClient overrides in unit tests.");

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options) =>
            throw new NotSupportedException();

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options,
            TRequest request) =>
            throw new NotSupportedException();

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options) =>
            throw new NotSupportedException();

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options,
            TRequest request) =>
            throw new NotSupportedException();
    }
}
