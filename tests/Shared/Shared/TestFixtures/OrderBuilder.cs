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

    public OrderBuilder WithItems(params (int quantity, long price)[] items)
    {
        _order.Items = items.Select(i => new OrderItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = "ProductName",
            Quantity = i.quantity,
            Price = i.price
        }).ToList();
        return this;
    }

    public Order Build() => _order;

    public static OrderBuilder Processing() => new OrderBuilder().WithState("Processing");
    public static OrderBuilder Shipped() => new OrderBuilder().WithState("Shipped");
    public static OrderBuilder Pending() => new OrderBuilder().WithState("Pending");
}