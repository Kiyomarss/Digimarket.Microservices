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
            ProductName = $"Product {i.productId:N}".Substring(0, 8),
            Quantity = i.quantity,
            Price = i.price
        }).ToList();
        return this;
    }

    public Order Build() => _order;

    // متدهای آماده — حالا OrderBuilder برمی‌گردانند نه Order!
    public static OrderBuilder Processing() => new OrderBuilder().WithState("Processing");
    public static OrderBuilder Shipped() => new OrderBuilder().WithState("Shipped");
    public static OrderBuilder Pending() => new OrderBuilder().WithState("Pending");
}