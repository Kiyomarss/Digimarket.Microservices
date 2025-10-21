using Catalog.Components.DTO;
using Catalog.Components.Repositories;

namespace Catalog.Components;

public class ProductGetterService : IProductGetterService
{
    readonly IProductRepository _productRepository;

    public ProductGetterService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<ProductDto>> GetProductByIds(List<Guid> productIds)
    {
        var catalog = await _productRepository.GetProductByIds(productIds);
        
        return catalog.Select(x => new ProductDto(x.Id, x.Name, x.Description, x.Stock, x.AttributesJson)).ToList();
    }
}