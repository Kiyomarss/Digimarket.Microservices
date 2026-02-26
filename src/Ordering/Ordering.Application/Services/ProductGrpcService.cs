// src/Ordering.Application/Services/ProductGrpcService.cs

using ProductGrpc;

namespace Ordering.Application.Services;

public class ProductGrpcService : IProductService
{
    private readonly ProductProtoService.ProductProtoServiceClient _client;

    public ProductGrpcService(ProductProtoService.ProductProtoServiceClient client)
    {
        _client = client;
    }

    public async Task<ReserveProductsResponse> ReserveProductsAsync(ReserveProductsRequest request, CancellationToken ct = default)
    {
        return await _client.ReserveProductsAsync(request, cancellationToken: ct);
    }
}