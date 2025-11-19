using Grpc.Core;

namespace Shared.Grpc;

public static class GrpcTestHelpers
{
    public static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(TResponse response)
    {
        return new AsyncUnaryCall<TResponse>(
                                             Task.FromResult(response),
                                             Task.FromResult(new Metadata()),
                                             () => Status.DefaultSuccess,
                                             () => new Metadata(),
                                             () => { });
    }

    public static AsyncUnaryCall<TResponse> CreateFailedCall<TResponse>(Status status, string message = "")
    {
        var exception = new RpcException(status, message);
        return new AsyncUnaryCall<TResponse>(
                                             Task.FromException<TResponse>(exception),
                                             Task.FromException<Metadata>(exception),
                                             () => status,
                                             () => new Metadata(),
                                             () => { });
    }
}