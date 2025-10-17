using Catalog.Components.DTO;

namespace Catalog.Components;

public interface ICatalogItemGetterService
{
    Task<List<CatalogItemDto>> GetCatalogItemByIds(List<Guid> catalogItemIds);
}