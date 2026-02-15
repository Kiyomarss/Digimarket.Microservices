using MassTransit;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines.Activities.Common;
using Ordering.Worker.StateMachines.Events;
using Shared.IntegrationEvents.Ordering;

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

            await Task.WhenAll(
                               context.Publish(new ReduceInventory(orderId)),
                               context.Publish(new RemoveBasket(orderId))
                              );

            await next.Execute(context);
        }
    }
}