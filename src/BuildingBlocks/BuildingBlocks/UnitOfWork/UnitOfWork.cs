using BuildingBlocks.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.UnitOfWork;

public sealed class UnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
{
    private readonly TContext _dbContext;
    private readonly IMediator _mediator;

    public UnitOfWork(
        TContext dbContext,
        IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
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
                await _mediator.Publish(domainEvent, ct);
            }
        }
    }
}