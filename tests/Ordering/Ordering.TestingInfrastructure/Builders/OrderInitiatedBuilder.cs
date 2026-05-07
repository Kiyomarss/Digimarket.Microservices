using BuildingBlocks.IntegrationEvents;
using BuildingBlocks.IntegrationEvents.Order;

namespace Ordering.TestingInfrastructure.Builders;

public class OrderInitiatedBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _userId = Guid.NewGuid();
    private List<OrderInitiated.OrderItemDto> _items =
        new(){ new(Guid.NewGuid(), 1) };
    private DateTime _date = DateTime.UtcNow;

    public OrderInitiatedBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public OrderInitiatedBuilder WithUser(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public OrderInitiatedBuilder WithItems(IEnumerable<OrderInitiated.OrderItemDto> items)
    {
        _items = items.ToList();
        return this;
    }

    public OrderInitiatedBuilder WithDate(DateTime date)
    {
        _date = date;
        return this;
    }

    public OrderInitiated Build()
    {
        return new OrderInitiated(
                                  _id,
                                  _userId,
                                  _items,
                                  _date);
    }
}