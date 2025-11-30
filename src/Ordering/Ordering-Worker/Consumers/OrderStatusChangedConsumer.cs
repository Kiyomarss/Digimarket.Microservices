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
            
            await _unitOfWork.SaveChangesAsync();
        }
    }
}