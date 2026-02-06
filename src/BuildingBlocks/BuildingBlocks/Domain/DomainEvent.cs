using MediatR;

namespace BuildingBlocks.Domain;

public abstract class DomainEvent : INotification
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
