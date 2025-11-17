using MassTransit;
using Ordering_Domain.Domain.RepositoryContracts;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.Consumers
{
    public class OrderInitiatedConsumer : IConsumer<OrderInitiated>
    {
        readonly IOrderRepository _orderRepository;

        public OrderInitiatedConsumer(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Consume(ConsumeContext<OrderInitiated> context)
        {
            var message = context.Message;
        }
    }
}