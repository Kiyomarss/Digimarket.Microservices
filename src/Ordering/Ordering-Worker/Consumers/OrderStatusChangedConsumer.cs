using BuildingBlocks.UnitOfWork;
using MassTransit;
using Ordering.Worker.StateMachines.Events;

namespace Ordering.Worker.Consumers
{
    public class OrderStatusChangedConsumer : IConsumer<OrderStatusChanged>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderStatusChangedConsumer> _logger;

        public OrderStatusChangedConsumer(ILogger<OrderStatusChangedConsumer> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
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
            await _unitOfWork.SaveChangesAsync();
        }
    }
}