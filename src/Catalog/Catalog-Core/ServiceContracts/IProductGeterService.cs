using Catalog.Core.DTO;

namespace Catalog.Core.ServiceContracts;

public interface IProductGetterService
{
    Task<List<ProductDto>> GetProductByIds(List<Guid> productIds);
}