using Catalog.Components.DTO;

namespace Catalog.Components;

public interface ICatalogItemUpdaterService
{
    Task AddCatalogItem(CreateCatalogItemDto dto);
    Task RemoveItem(Guid id);
}