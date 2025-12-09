using BuildingBlocks.Common.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Common.Interceptors;

public sealed class DomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;

    public DomainEventsInterceptor(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext is null) return await base.SavedChangesAsync(eventData, result, cancellationToken);

        var entitiesWithEvents = dbContext.ChangeTracker
                                          .Entries<Entity>()
                                          .Where(e => e.Entity.DomainEvents.Any())
                                          .Select(e => e.Entity)
                                          .ToArray();

        var domainEvents = entitiesWithEvents
                           .SelectMany(e => e.DomainEvents)
                           .ToArray();

        foreach (var entity in entitiesWithEvents)
            entity.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}