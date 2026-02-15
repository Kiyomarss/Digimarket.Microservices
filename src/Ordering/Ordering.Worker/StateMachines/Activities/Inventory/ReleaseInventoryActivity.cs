using MassTransit;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines.Activities.Common;
using Ordering.Worker.StateMachines.Events;

namespace Ordering.Worker.StateMachines.Activities.Inventory;

public class ReleaseInventoryActivity :
    BaseActivity<OrderState, CancelOrder>
{
    public override async Task Execute(
        BehaviorContext<OrderState, CancelOrder> context,
        IBehavior<OrderState, CancelOrder> next)
    {
        await context.Publish(new ReleaseInventory(context.Saga.CorrelationId));

        await next.Execute(context);
    }
}