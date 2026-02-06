using Ordering_Domain.Domain.Enum;

namespace Ordering_Domain.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; }
    public OrderState State { get; set; } = OrderState.Pending;
    public List<OrderItem> Items { get; set; } = new();
    public long TotalPrice => Items.Sum(x => x.Price * x.Quantity);
    
    public Guid UserId { get; set; }
    
    private void ChangeStateInternal(OrderState newState)
    {
        State = newState;
    }
    
    public void Pay() => ChangeStateInternal(OrderState.Paid);

    public void Ship() => ChangeStateInternal(OrderState.Shipped);

    public void Cancel() => ChangeStateInternal(OrderState.Cancelled);

}