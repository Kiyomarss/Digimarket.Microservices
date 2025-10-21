using Catalog.Components.DTO;

namespace Catalog.Components;

public interface IProductGetterService
{
    Task<List<ProductDto>> GetProductByIds(List<Guid> productIds);
}