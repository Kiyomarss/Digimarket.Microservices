using Basket_Application.Basket.Commands.RemoveBasket;
using BuildingBlocks.IntegrationEvents.Basket;
using BuildingBlocks.IntegrationEvents.Order;
using MassTransit;
using MediatR;

namespace Basket_Application.Basket.Consumers
{
    public class RemoveBasketConsumer : IConsumer<RemoveBasket>
    {
        private readonly ISender _sender;

        public RemoveBasketConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<RemoveBasket> context)
        {
            await _sender.Send(new RemoveBasketCommand()
            {
                UserId = context.Message.UserId
            });
        }
    }
}