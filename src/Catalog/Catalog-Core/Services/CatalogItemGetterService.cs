using Catalog.Components.DTO;
using Catalog.Components.Repositories;

namespace Catalog.Components;

public class CatalogItemGetterService : ICatalogItemGetterService
{
    readonly ICatalogItemRepository _catalogItemRepository;

    public CatalogItemGetterService(ICatalogItemRepository catalogItemRepository)
    {
        _catalogItemRepository = catalogItemRepository;
    }

    public async Task<List<CatalogItemDto>> GetCatalogItemByIds(List<Guid> catalogItemIds)
    {
        var catalog = await _catalogItemRepository.GetCatalogItemByIds(catalogItemIds);
        
        return catalog.Select(x => new CatalogItemDto(x.Id, x.Name, x.Description, x.Stock, x.AttributesJson)).ToList();
    }
}