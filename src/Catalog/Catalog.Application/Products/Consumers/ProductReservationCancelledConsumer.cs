using BuildingBlocks.IntegrationEvents;
using Catalog.Application.Products.Commands.ProductReservationCancelled;
using MassTransit;
using MediatR;

namespace Catalog.Application.Products.Consumers
{
    public class ProductReservationCancelledConsumer : IConsumer<ProductReservationCancelled>
    {
        private readonly ISender _sender;

        public ProductReservationCancelledConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<ProductReservationCancelled> context)
        {
            await _sender.Send(new ProductReservationCancelledCommand(context.Message.Items.Select(x => new OrderItemDto(x.ProductId, x.Quantity))));
        }
    }
}