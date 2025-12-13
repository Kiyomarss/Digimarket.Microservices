using BuildingBlocks.IntegrationEvents;
using BuildingBlocks.UnitOfWork;
using MassTransit;
using MediatR;
using Ordering.Application.Orders.Commands.CreateOrder;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Api.Consumers
{
    public class OrderCreatedIntegrationEventConsumer : IConsumer<OrderCreatedIntegrationEvent>
    {

        public OrderCreatedIntegrationEventConsumer()
        {
        }

        public Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
        {
            var message = context.Message;

            return Task.CompletedTask;
        }
    }
}