using Grpc.Core;
using Part;

namespace AutoserviceOrders.Tests.Common;

internal sealed class FakePartServiceClient : PartService.PartServiceClient
{
    private readonly IReadOnlyDictionary<int, PartReply> _catalog;

    public FakePartServiceClient(IReadOnlyDictionary<int, PartReply>? catalog = null)
        : base(new StubCallInvoker())
    {
        _catalog = catalog ?? new Dictionary<int, PartReply>();
    }

    public override AsyncUnaryCall<PartReply> GetPartAsync(GetPartRequest request, CallOptions options)
    {
        if (!_catalog.TryGetValue(request.Id, out var reply))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Part {request.Id} not found"));
        }

        return GrpcResponses.Success(reply);
    }

    private sealed class StubCallInvoker : CallInvoker
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options,
            TRequest request) =>
            throw new NotSupportedException("Use FakePartServiceClient overrides in unit tests.");

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
