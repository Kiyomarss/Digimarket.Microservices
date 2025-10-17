using Microsoft.EntityFrameworkCore;

namespace Catalog.Components.Repositories;

public class CatalogItemItemRepository : ICatalogItemRepository
{
    readonly CatalogDbContext _db;

    public CatalogItemItemRepository(CatalogDbContext dbContext)
    {
        _db = dbContext;
    }
    
    public async Task<List<CatalogItem>> GetCatalogItemByIds(List<Guid> catalogItemIds)
    {
        return await _db.Set<CatalogItem>()
                        .Where(x => catalogItemIds.Contains(x.Id))
                        .ToListAsync();;
    }
    
    public async Task<CatalogItem?> FindCatalogItemById(Guid id)
    {
        return await _db.Set<CatalogItem>().FindAsync(id);
    }
    
    public async Task<bool> DeleteCatalogItem(Guid id)
    {
        var rowsDeleted = await _db.Set<CatalogItem>()
                                   .Where(b => b.Id == id)
                                   .ExecuteDeleteAsync();

        return rowsDeleted > 0;
    }
    
    public async Task AddItemToCatalog(CatalogItem item)
    {
        await _db.Set<CatalogItem>().AddAsync(item);
        await _db.SaveChangesAsync();
    }
}