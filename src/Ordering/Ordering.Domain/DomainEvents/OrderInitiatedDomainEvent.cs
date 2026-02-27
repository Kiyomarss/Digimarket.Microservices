using BuildingBlocks.Domain;

namespace Ordering_Domain.DomainEvents;

public sealed class OrderInitiatedDomainEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid UserId { get; }
    public IReadOnlyCollection<OrderItemSnapshot> Items { get; }

    public OrderInitiatedDomainEvent(
        Guid orderId,
        Guid userId,
        IReadOnlyCollection<OrderItemSnapshot> items)
    {
        OrderId = orderId;
        UserId = userId;
        Items = items;
    }

    public sealed record OrderItemSnapshot(
        Guid ProductId,
        int Quantity);
}