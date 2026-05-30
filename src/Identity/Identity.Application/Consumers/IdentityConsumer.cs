using BuildingBlocks.IntegrationEvents.Order;
using MassTransit;
using MediatR;

namespace Identity.Application.Consumers
{
    public class IdentityConsumer : IConsumer<OrderCanceled>
    {
        private readonly ISender _sender;

        public IdentityConsumer(ISender sender)
        {
            _sender = sender;
        }

        public Task Consume(ConsumeContext<OrderCanceled> context)
        {
            return Task.CompletedTask;
        }
    }
}