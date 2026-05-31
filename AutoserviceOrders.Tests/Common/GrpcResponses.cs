using Grpc.Core;
using Part;

namespace AutoserviceOrders.Tests.Common;

internal static class GrpcResponses
{
    public static AsyncUnaryCall<PartReply> Success(PartReply reply) =>
        new(
            Task.FromResult(reply),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

    public static AsyncUnaryCall<PartReply> Failure(RpcException exception) =>
        new(
            Task.FromException<PartReply>(exception),
            Task.FromResult(new Metadata()),
            () => exception.Status,
            () => new Metadata(),
            () => { });
}
