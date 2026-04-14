using Ordering_Domain.Domain.Entities;
using Ordering_Domain.ValueObjects;

namespace Shared.TestFixtures;

public sealed class OrderEmptyBuilder
{
    private readonly Order _order;

    public OrderEmptyBuilder()
    {
        // Order بدون آیتم می‌سازیم
        _order = Order.Create(TestGuids.Guid3, Enumerable.Empty<OrderItemData>());
    }

    public OrderEmptyBuilder WithItem(Guid productId, long price, int quantity)
    {
        _order.AddItem(productId, price, quantity);
        return this;
    }

    public OrderEmptyBuilder Canceled()
    {
        _order.Canceled();
        return this;
    }

    public Order Build() => _order;
}