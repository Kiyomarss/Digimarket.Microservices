using MassTransit;
using Ordering.Core.Domain.Entities;
using Ordering.Core.Domain.RepositoryContracts;
using Ordering.Core.DTO;

namespace Ordering.Core.Consumers
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

            await _orderRepository.Update();
        }
    }
}