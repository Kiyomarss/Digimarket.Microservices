using Grpc.Core;
using System.Threading.Tasks;

namespace Ordering.UnitTests.TestHelpers;

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