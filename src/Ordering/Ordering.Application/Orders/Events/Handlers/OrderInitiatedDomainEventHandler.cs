using BuildingBlocks.IntegrationEvents;
using MassTransit;
using MediatR;
using Ordering_Domain.DomainEvents;

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
        var message = new OrderInitiated(
                                         notification.OrderId,
                                         notification.UserId,
                                         notification.Items
                                                     .Select(x => new OrderInitiated.OrderItemDto(
                                                                                                  x.ProductId,
                                                                                                  x.Quantity))
                                                     .ToList(),
                                         DateTime.UtcNow);

        await _publish.Publish(message, ct);
    }
}
