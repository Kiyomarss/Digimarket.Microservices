using Ordering_Domain.Domain.Entities;
using Ordering_Domain.ValueObjects;
using Shared;

namespace Ordering.TestingInfrastructure.Builders;

public sealed class OrderBuilder
{
    private readonly Order _order;

    public OrderBuilder()
    {
        var items = new List<OrderItemData>()
        {
            new(TestGuids.Guid4, 10, 5),
            new(TestGuids.Guid5, 40, 5)
        };

        _order = Order.Create(TestGuids.Guid3, items);
    }
    
    public OrderBuilder Canceled()
    {
        _order.Canceled();
        return this;
    }
    
    public OrderBuilder Paid()
    {
        _order.MarkAsPaid();
        return this;
    }


    public Order Build() => _order;
}