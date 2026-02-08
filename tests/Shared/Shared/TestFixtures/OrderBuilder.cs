using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.Enum;
using Shared;

public sealed class OrderBuilder
{
    private readonly Order _order;

    public OrderBuilder()
    {
        _order = Order.Create(TestGuids.Guid3);
    }

    public OrderBuilder WithItems(params (int quantity, long price)[] items)
    {
        foreach (var item in items)
        {
            _order.AddItem(
                           Guid.NewGuid(),
                           $"Product {Guid.NewGuid():N}".Substring(0, 8),
                           item.price,
                           item.quantity);
        }

        return this;
    }

    public OrderBuilder Paid()
    {
        _order.Pay();

        return this;
    }

    public Order Build() => _order;
}