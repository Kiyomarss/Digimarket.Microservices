using BuildingBlocks.UnitOfWork;
using MassTransit;
using MediatR;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Application.Orders.Commands.PayOrder;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Api.Consumers
{
    public class OrderStatusChangedConsumer : IConsumer<OrderStatusChanged>
    {
        private readonly ISender _sender;

        public OrderStatusChangedConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<OrderStatusChanged> context)
        {
            var message = context.Message;
            var command = new PayOrderCommand
            {
                Id = message.Id
            };

            await _sender.Send(command);
        }
    }
}