using BuildingBlocks.Domain;

namespace Ordering_Domain.DomainEvents;

public sealed class OrderCancelledAfterPaymentDomainEvent : DomainEvent
{
    public Guid OrderId { get; }

    public IReadOnlyCollection<OrderItemSnapshot> Items { get; }

    public OrderCancelledAfterPaymentDomainEvent(
        Guid orderId,
        IReadOnlyCollection<OrderItemSnapshot> items)
    {
        OrderId =  orderId;
        Items = items;
    }

    public sealed record OrderItemSnapshot(
        Guid ProductId,
        int Quantity);
}