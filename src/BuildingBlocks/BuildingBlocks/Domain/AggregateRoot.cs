using System.ComponentModel.DataAnnotations.Schema;

namespace BuildingBlocks.Domain;

public abstract class AggregateRoot
{
    [NotMapped]
    private readonly List<DomainEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents;

    protected void AddDomainEvent(DomainEvent evt)
        => _domainEvents.Add(evt);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}

