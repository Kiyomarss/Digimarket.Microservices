using Ordering_Domain.Domain.Enum;

namespace Ordering_Domain.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
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
    
    public void Pay() => ChangeStateInternal(OrderState.Paid);

    public void Ship() => ChangeStateInternal(OrderState.Shipped);

    public void Cancel() => ChangeStateInternal(OrderState.Cancelled);

    public void AddItem(
        Guid productId,
        string productName,
        long price,
        int quantity)
    {
        if(quantity <= 0)
            throw new InvalidOperationException("Quantity must be positive");

        _items.Add(OrderItem.Create(
                                    Id,
                                    productId,
                                    productName,
                                    price,
                                    quantity));
    }
    
    public static Order Create(Guid userId)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = DateTime.UtcNow,
            State = OrderState.Pending
        };
    }
}