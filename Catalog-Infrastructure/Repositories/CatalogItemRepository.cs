using Microsoft.EntityFrameworkCore;

namespace Catalog.Components.Repositories;

public class CatalogItemRepository : ICatalogItemRepository
{
    readonly CatalogDbContext _db;

    public CatalogItemRepository(CatalogDbContext dbContext)
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
    
    public async Task AddCatalogItem(CatalogItem catalogItem)
    {
        await _db.Set<CatalogItem>().AddAsync(catalogItem);
        await _db.SaveChangesAsync();
    }
}