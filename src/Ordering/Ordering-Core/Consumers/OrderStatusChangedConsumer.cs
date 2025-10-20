using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Components.DTO;

namespace Ordering.Components.Consumers
{
    public class OrderStatusChangedConsumer : IConsumer<OrderStatusChanged>
    {
        private readonly OrderingDbContext _dbContext;
        private readonly ILogger<OrderStatusChangedConsumer> _logger;

        public OrderStatusChangedConsumer(OrderingDbContext dbContext, ILogger<OrderStatusChangedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderStatusChanged> context)
        {
            var message = context.Message;

            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == message.Id);

            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found when trying to update status to {NewStatus}", 
                                   message.Id, message.OrderState);
                return;
            }

            order.State = message.OrderState;

            await _dbContext.SaveChangesAsync();
        }
    }
}