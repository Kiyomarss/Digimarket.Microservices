using Catalog.Components.DTO;

namespace Catalog.Components;

public interface IProductUpdaterService
{
    Task AddProduct(CreateProductDto dto);
    Task RemoveItem(Guid id);
}