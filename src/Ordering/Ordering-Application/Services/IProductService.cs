// src/Ordering.Application/Services/IProductService.cs

using ProductGrpc;

namespace Ordering.Core.Services;

public interface IProductService
{
    Task<GetProductsResponse> GetProductsByIdsAsync(IEnumerable<string> productIds, CancellationToken ct = default);
}