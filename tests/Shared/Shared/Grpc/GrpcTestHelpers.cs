using Grpc.Core;

namespace Shared.Grpc;

public static class GrpcTestHelpers
{
    public static AsyncUnaryCall<T> CreateAsyncUnaryCall<T>(T response)
    {
        return new AsyncUnaryCall<T>(
                                     Task.FromResult(response),
                                     Task.FromResult(new Metadata()),
                                     () => Status.DefaultSuccess,
                                     () => new Metadata(),
                                     () => { });
    }
}