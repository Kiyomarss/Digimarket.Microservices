using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.UnitOfWork;

public sealed class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _dbContext;

    public UnitOfWork(TContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}