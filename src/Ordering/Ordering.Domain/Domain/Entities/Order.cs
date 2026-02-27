using BuildingBlocks.Domain;
using BuildingBlocks.Exceptions.Domain;
using Ordering_Domain.Domain.Enum;
using Ordering_Domain.DomainEvents;
using Ordering_Domain.ValueObjects;

namespace Ordering_Domain.Domain.Entities;

public class Order : AggregateRoot
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public OrderState State { get; set; } = OrderState.Pending;
    
    private readonly List<OrderItem> _items = new();

    public IReadOnlyCollection<OrderItem> Items => _items;
    
    public long TotalPrice => Items.Sum(x => x.Price * x.Quantity);
    
    public Guid UserId { get; set; }
    
    private void ChangeStateInternal(OrderState newState)
    {
        State = newState;
    }
    
    public void Canceled() => ChangeStateInternal(OrderState.Canceled);

    public void AddItem(
        Guid productId,
        long price,
        int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive");

        _items.Add(OrderItem.Create(
                                    Id,
                                    productId,
                                    price,
                                    quantity));
    }
    
    public static Order Create(
        Guid userId,
        IEnumerable<OrderItemData> items)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = DateTime.UtcNow,
            State = OrderState.Pending
        };

        foreach (var item in items)
            order.AddItem(item.ProductId, item.Price, item.Quantity);

        order.AddDomainEvent(
                             new OrderInitiatedDomainEvent(
                                                           order.Id,
                                                           order.UserId,
                                                           order._items.Select(i => new OrderInitiatedDomainEvent.OrderItemSnapshot(
                                                                                                                                    i.ProductId,
                                                                                                                                    i.Quantity)).ToList()));

        return order;
    }
}