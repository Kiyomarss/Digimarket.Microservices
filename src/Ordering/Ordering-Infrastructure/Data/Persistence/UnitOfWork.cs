using BuildingBlocks.UnitOfWork;
using Ordering_Infrastructure.Data.DbContext;

namespace Ordering_Infrastructure.Data.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrderingDbContext _db;

    public UnitOfWork(OrderingDbContext db)
    {
        _db = db;
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
}