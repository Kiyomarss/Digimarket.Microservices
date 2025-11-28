using BuildingBlocks.UnitOfWork;
using MassTransit;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.Consumers
{
    public class OrderInitiatedConsumer : IConsumer<OrderInitiated>
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderInitiatedConsumer(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<OrderInitiated> context)
        {
            var message = context.Message;
            
            await _unitOfWork.SaveChangesAsync();
        }
    }
}