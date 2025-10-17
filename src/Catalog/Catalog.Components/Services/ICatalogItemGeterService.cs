using Catalog.Components.Contracts;

namespace Catalog.Components;

public interface ICatalogItemGetterService
{
    Task<List<CatalogItemDto>> GetCatalogItemByIds(List<Guid> catalogItemIds);
}