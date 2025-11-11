using Grpc.Core;
using ProductGrpc;
using System.Threading.Tasks;

namespace Ordering.ApiTests.Utilities;

public class FakeProductGrpcClient : ProductProtoService.ProductProtoServiceClient
{
    public override AsyncUnaryCall<GetProductsResponse> GetProductsByIdsAsync(
        GetProductsRequest request, CallOptions options)
    {
        var response = new GetProductsResponse();
        foreach (var id in request.ProductIds)
        {
            response.Products.Add(new ProductInfo
            {
                ProductId = id,
                ProductName = "Test Product " + id,
                Price = 100
            });
        }

        // AsyncUnaryCall نیاز به Task برای پاسخ و CancellationToken/Headers و غیره دارد
        return new AsyncUnaryCall<GetProductsResponse>(
                                                       Task.FromResult(response),       // ResponseAsync
                                                       Task.FromResult(new Metadata()), // ResponseHeadersAsync
                                                       () => Status.DefaultSuccess,    // GetStatus
                                                       () => new Metadata(),           // GetTrailers
                                                       () => { }                       // Dispose
                                                      );
    }
}