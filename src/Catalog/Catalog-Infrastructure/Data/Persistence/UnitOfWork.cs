using BuildingBlocks.UnitOfWork;
using Catalog_Infrastructure.Data.DbContext;

namespace Catalog_Infrastructure.Data.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly CatalogDbContext _db;

    public UnitOfWork(CatalogDbContext db)
    {
        _db = db;
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
}