using BuildingBlocks.CQRS;
using Catalog_Damain.RepositoryContracts;

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
        var products = await _productRepository.GetProductByIds(query.ProductIds);

        var result = products.Select(p => new ProductDto(p.Id, p.Name, p.Price)).ToList();

        return new GetProductsByIdsResponse(result);
    }
}