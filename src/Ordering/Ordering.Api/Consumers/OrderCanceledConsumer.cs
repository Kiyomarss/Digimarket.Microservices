using BuildingBlocks.IntegrationEvents;
using MassTransit;
using MediatR;
using Ordering.Application.Orders.Commands.OrderCancelled;

namespace Ordering.Api.Consumers
{
    public class OrderCanceledConsumer : IConsumer<OrderCanceled>
    {
        private readonly ISender _sender;

        public OrderCanceledConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<OrderCanceled> context)
        {
            var message = context.Message;
            var command = new OrderCanceledCommand
            {
                Id = message.Id
            };

            await _sender.Send(command);
        }
    }
}