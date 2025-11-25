using BuildingBlocks.UnitOfWork;
using MassTransit;
using Ordering_Domain.Domain.RepositoryContracts;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.Consumers
{
    public class OrderInitiatedConsumer : IConsumer<OrderInitiated>
    {
        readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderInitiatedConsumer(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<OrderInitiated> context)
        {
            var message = context.Message;
            
            await _unitOfWork.SaveChangesAsync();
        }
    }
}