// src/Ordering.Application/Services/IProductService.cs

using ProductGrpc;

namespace Ordering.Application.ServiceContracts;

public interface IProductService
{
    Task<ReserveProductsResponse> ReserveProductsAsync(ReserveProductsRequest request, CancellationToken ct = default);
}