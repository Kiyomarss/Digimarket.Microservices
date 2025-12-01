// Shared/TestFixtures/OrderBuilder.cs

using Ordering_Domain.Domain.Entities;
using Shared;

public class OrderBuilder
{
    private readonly Order _order = new()
    {
        Id = Guid.NewGuid(),
        UserId = TestGuids.Guid3,
        Customer = "Test Customer",
        State = "Pending",
        Date = DateTime.Now,
        Items = new List<OrderItem>()
    };

    public OrderBuilder WithState(string state)
    {
        _order.State = state;
        return this;
    }

    public OrderBuilder WithItems(params (Guid productId, int quantity, long price)[] items)
    {
        _order.Items = items.Select(i => new OrderItem
        {
            ProductId = i.productId,
            Quantity = i.quantity,
            Price = i.price
        }).ToList();
        return this;
    }

    public Order Build() => _order;

    // متدهای آماده
    public static Order Processing() => new OrderBuilder().WithState("Processing").Build();
    public static Order Shipped() => new OrderBuilder().WithState("Shipped").Build();
    public static Order Pending() => new OrderBuilder().WithState("Pending").Build();
}