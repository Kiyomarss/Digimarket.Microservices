using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.Enum;
using Shared;

public class OrderBuilder
{
    private readonly Order _order = new()
    {
        Id = Guid.NewGuid(),
        UserId = TestGuids.Guid3,
        Customer = "Test Customer",
        Date = DateTime.Now,
        Items = new List<OrderItem>()
    };

    public OrderBuilder WithState(OrderState state)
    {
        _order.State = state;
        return this;
    }

    public OrderBuilder WithItems(params (int quantity, long price)[] items)
    {
        _order.Items = items.Select(i => new OrderItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = $"Product {Guid.NewGuid():N}".Substring(0, 8),
            Quantity = i.quantity,
            Price = i.price
        }).ToList();
        return this;
    }

    public OrderBuilder WithUserId(Guid userId)
    {
        _order.UserId = userId;
        return this;
    }

    public Order Build() => _order;

    // متدهای آماده
    public static OrderBuilder Processing() => new OrderBuilder().WithState(OrderState.Processing);
    public static OrderBuilder Shipped() => new OrderBuilder().WithState(OrderState.Shipped);
    public static OrderBuilder Pending() => new OrderBuilder().WithState(OrderState.Pending);
}