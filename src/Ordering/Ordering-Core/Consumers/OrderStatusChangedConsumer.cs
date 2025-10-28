using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Core.Domain.Entities;
using Ordering.Core.Domain.RepositoryContracts;
using Ordering.Core.DTO;

namespace Ordering.Core.Consumers
{
    public class OrderStatusChangedConsumer : IConsumer<OrderStatusChanged>
    {
        readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderStatusChangedConsumer> _logger;

        public OrderStatusChangedConsumer(IOrderRepository orderRepository, ILogger<OrderStatusChangedConsumer> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderStatusChanged> context)
        {
            var message = context.Message;

            var a = _orderRepository.FindOrderById(Guid.Parse("9336d6b2-68cf-48f8-81c0-5eb335f4571e"));
            var order = new Order();

            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found when trying to update status to {NewStatus}", 
                                   message.Id, message.OrderState);
                return;
            }

            order.State = message.OrderState;

            await _orderRepository.Update();
        }
    }
}