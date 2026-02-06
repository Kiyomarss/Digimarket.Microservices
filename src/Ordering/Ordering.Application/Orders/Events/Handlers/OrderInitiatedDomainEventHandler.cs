using MassTransit;
using MediatR;
using Ordering_Domain.DomainEvents;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Application.Orders.Events.Handlers;

public class OrderInitiatedDomainEventHandler
    : INotificationHandler<OrderInitiatedDomainEvent>
{
    private readonly IPublishEndpoint _publish;

    public OrderInitiatedDomainEventHandler(IPublishEndpoint publish)
    {
        _publish = publish;
    }

    public async Task Handle(
        OrderInitiatedDomainEvent notification,
        CancellationToken ct)
    {
        await _publish.Publish(new OrderInitiated
        {
            Id = notification.OrderId,
            Date = DateTime.UtcNow
        }, ct);
    }
}
