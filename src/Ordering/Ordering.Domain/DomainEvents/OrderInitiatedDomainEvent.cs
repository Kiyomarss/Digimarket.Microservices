using BuildingBlocks.Domain;

namespace Ordering_Domain.DomainEvents;

public sealed class OrderInitiatedDomainEvent : DomainEvent
{
    public Guid OrderId { get; }

    public OrderInitiatedDomainEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}