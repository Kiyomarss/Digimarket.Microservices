using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;

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
    
    public static GrpcChannel CreateGrpcChannel<TFactory>(this WebApplicationFactory<TFactory> factory)
        where TFactory : class
    {
        return GrpcChannel.ForAddress(factory.Server.BaseAddress, new GrpcChannelOptions
        {
            HttpClient = factory.CreateDefaultClient()
        });
    }
}