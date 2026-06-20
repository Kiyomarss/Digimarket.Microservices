using BuildingBlocks.IntegrationEvents.Order;
using MassTransit;
using MediatR;
using Ordering_Domain.DomainEvents;

namespace Ordering.Application.Orders.Events.Handlers;

public class OrderCancelledAfterPaymentDomainEventHandler
    : INotificationHandler<OrderCancelledAfterPaymentDomainEvent>
{
    private readonly IPublishEndpoint _publish;

    public OrderCancelledAfterPaymentDomainEventHandler(IPublishEndpoint publish)
    {
        _publish = publish;
    }

    public async Task Handle(
        OrderCancelledAfterPaymentDomainEvent notification,
        CancellationToken ct)
    {
        var message = new OrderCancelledAfterPayment(
                                                     notification.OrderId,
                                                     notification.Items
                                                                 .Select(x => new OrderCancelledAfterPayment.ProductItemsDto(
                                                                                                                             x.ProductId,
                                                                                                                             x.Quantity)).ToList());

        await _publish.Publish(message, ct);
    }
}