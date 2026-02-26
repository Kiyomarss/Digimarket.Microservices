// src/Ordering.Application/Services/IProductService.cs

using ProductGrpc;

namespace Ordering.Application.Services;

public interface IProductService
{
    Task<ReserveProductsResponse> ReserveProductsAsync(ReserveProductsRequest request, CancellationToken ct = default);
}