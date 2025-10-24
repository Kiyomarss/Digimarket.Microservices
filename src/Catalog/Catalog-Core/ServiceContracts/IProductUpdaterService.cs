using Catalog.Core.DTO;

namespace Catalog.Core.ServiceContracts;

public interface IProductUpdaterService
{
    Task AddProduct(CreateProductDto dto);
    Task RemoveItem(Guid id);
}