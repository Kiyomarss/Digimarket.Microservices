using BuildingBlocks.Common.Entities;
using Ordering_Domain.Domain.Events;

namespace Ordering_Domain.Domain.Entities;

public class Order : Entity, IAggregateRoot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; }
    public string State { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public long TotalPrice => Items.Sum(x => x.Price * x.Quantity);
    
    public Guid UserId { get; set; }
    public string Customer { get; set; } = null!;
    
    public void RaiseOrderCreatedEvent()
    {
        AddDomainEvent(new OrderCreatedDomainEvent(
                                                   Id: Id,
                                                   UserId: UserId,
                                                   Date: Date,
                                                   Customer: Customer,
                                                   Items: Items
                                                  ));
    }
}