using BuildingBlocks.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.UnitOfWork;

public sealed class UnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
{
    private readonly TContext _dbContext;
    private readonly IPublisher _publisher;

    public UnitOfWork(
        TContext dbContext,
        IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(cancellationToken);
        
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEvents(CancellationToken ct)
    {
        var aggregates = _dbContext.ChangeTracker
                                   .Entries<AggregateRoot>()
                                   .Where(e => e.Entity.DomainEvents.Any())
                                   .Select(e => e.Entity)
                                   .ToList();

        foreach (var aggregate in aggregates)
        {
            var events = aggregate.DomainEvents.ToList();

            aggregate.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await _publisher.Publish(domainEvent, ct);
            }
        }
    }
}