using BuildingBlocks.IntegrationEvents;
using MassTransit;
using MediatR;
using Ordering_Domain.Domain.Events;

namespace Ordering.Application.Orders.Commands.CreateOrder.Events;

public class OrderCreatedDomainEventHandler 
    : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderCreatedDomainEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(OrderCreatedDomainEvent evt, CancellationToken ct)
    {
        var integrationEvent = new OrderCreatedIntegrationEvent(evt.Id);
        await _publishEndpoint.Publish(integrationEvent, ct);
    }
}

