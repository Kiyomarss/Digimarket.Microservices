using BuildingBlocks.IntegrationEvents;
using MassTransit;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines.Activities.Common;
using Ordering.Worker.StateMachines.Contracts.Dtos;
using Ordering.Worker.StateMachines.Contracts.Events;

namespace Ordering.Worker.StateMachines.Activities.Initialize
{
    public class InitializeOrderActivity :
        BaseActivity<OrderState, OrderInitiated>
    {
        public override async Task Execute(
            BehaviorContext<OrderState, OrderInitiated> context,
            IBehavior<OrderState, OrderInitiated> next)
        {
            context.Saga.Date = context.Message.Date;

            var orderId = context.Saga.CorrelationId;
            var items = context.Message.Items.Select(x => new OrderItemDto(x.ProductId, x.Quantity));

            await Task.WhenAll(
                               context.Publish(new ReduceInventory(items)),
                               context.Publish(new RemoveBasket(orderId))
                              );

            await next.Execute(context);
        }
    }
}