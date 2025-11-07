using Basket.Infrastructure.Data.DbContext;
using BuildingBlocks.UnitOfWork;

namespace Basket.Infrastructure.Data.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly BasketDbContext _db;

    public UnitOfWork(BasketDbContext db)
    {
        _db = db;
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
}