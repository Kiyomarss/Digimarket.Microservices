using BuildingBlocks.IntegrationEvents;
using MassTransit;
using Ordering.Application.RepositoryContracts.Realtime;

namespace Ordering.Application.Orders.Consumers
{
    public class OrderStatusChangedConsumer : IConsumer<OrderStatusChangedIntegrationEvent>
    {
        private readonly IOrderStatusNotifier _notifier;

        public OrderStatusChangedConsumer(IOrderStatusNotifier notifier)
        {
            _notifier = notifier;
        }

        public async Task Consume(ConsumeContext<OrderStatusChangedIntegrationEvent> context)
        {
            await _notifier.NotifyAsync(
                                        context.Message.Id,
                                        context.Message.Status
                                       );
        }
    }
}