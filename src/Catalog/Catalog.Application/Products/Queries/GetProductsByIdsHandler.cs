using BuildingBlocks.CQRS;
using Catalog.Application.RepositoryContracts;

namespace Catalog.Application.Products.Queries;

public class GetProductsByIdsHandler 
    : IQueryHandler<GetProductsByIdsQuery, GetProductsByIdsResponse>
{
    private readonly IProductRepository _productRepository;

    public GetProductsByIdsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<GetProductsByIdsResponse> Handle(
        GetProductsByIdsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetProductByIds(query.ProductIds, cancellationToken);
        
        return new GetProductsByIdsResponse(products);
    }
}