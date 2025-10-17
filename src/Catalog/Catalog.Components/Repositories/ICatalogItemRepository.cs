namespace Catalog.Components.Repositories;

public interface ICatalogItemRepository
{
    Task<List<CatalogItem>> GetCatalogItemByIds(List<Guid> catalogItemIds);
    
    Task AddItemToCatalog(CatalogItem item);
    
    Task<CatalogItem?> FindCatalogItemById(Guid id);

    Task<bool> DeleteCatalogItem(Guid id);
}