using Grpc.Core;
using Grpc.Core.Interceptors;
using BuildingBlocks.Exceptions.Application;

public sealed class GrpcExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (ExternalServiceException ex)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}