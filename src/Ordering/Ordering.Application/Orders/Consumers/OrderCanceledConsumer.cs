using BuildingBlocks.IntegrationEvents;
using MassTransit;
using MediatR;
using Ordering.Application.Orders.Commands.OrderCancelled;

namespace Ordering.Application.Orders.Consumers
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
            await _sender.Send(new OrderCanceledCommand()
            {
                Id = context.Message.Id
            });
        }
    }
}