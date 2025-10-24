using Catalog.Core.Domain.RepositoryContracts;
using Grpc.Core;
using ProductGrpc;

namespace Catalog.Core.Services;

public class ProductGrpcService : ProductProtoService.ProductProtoServiceBase
{
    private readonly IProductRepository _productRepository;

    public ProductGrpcService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public override async Task<GetProductsResponse> GetProductsByIds(GetProductsRequest request, ServerCallContext context)
    {
        var productIds = request.ProductIds
                                .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
                                .Where(g => g != Guid.Empty)
                                .ToList();

        var products = await _productRepository.GetProductByIds(productIds);

        var response = new GetProductsResponse();

        foreach (var p in products)
        {
            response.Products.Add(new ProductInfo
            {
                ProductId = p.Id.ToString(),
                ProductName = p.Name,
                Price = p.Price
            });
        }

        return response;
    }
}