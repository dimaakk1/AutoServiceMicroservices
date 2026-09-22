using Grpc.Core;

namespace ReviewsService.Tests.Common;

internal static class GrpcResponses
{
    public static AsyncUnaryCall<TResponse> Success<TResponse>(TResponse reply) =>
        new(
            Task.FromResult(reply),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

    public static AsyncUnaryCall<TResponse> NotFound<TResponse>() =>
        new(
            Task.FromException<TResponse>(new RpcException(new Status(StatusCode.NotFound, "not found"))),
            Task.FromResult(new Metadata()),
            () => new Status(StatusCode.NotFound, "not found"),
            () => new Metadata(),
            () => { });
}
