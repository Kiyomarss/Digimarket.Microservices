// src/Ordering.Application/Services/IProductService.cs

using ProductGrpc;

namespace Ordering.Application.Services;

public interface IProductService
{
    Task<GetProductsResponse> GetProductsByIdsAsync(IEnumerable<string> productIds, CancellationToken ct = default);
}