// src/Ordering.Application/Services/ProductGrpcService.cs

using ProductGrpc;

namespace Ordering.Core.Services;

public class ProductGrpcService : IProductService
{
    private readonly ProductProtoService.ProductProtoServiceClient _client;

    public ProductGrpcService(ProductProtoService.ProductProtoServiceClient client)
    {
        _client = client;
    }

    public async Task<GetProductsResponse> GetProductsByIdsAsync(IEnumerable<string> productIds, CancellationToken ct = default)
    {
        var request = new GetProductsRequest();
        request.ProductIds.AddRange(productIds);

        return await _client.GetProductsByIdsAsync(request, cancellationToken: ct);
    }
}