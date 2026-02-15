using MassTransit;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines.Activities.Common;
using Ordering.Worker.StateMachines.Events;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.StateMachines.Activities.Processing;

public class PublishOrderPaidActivity :
    BaseActivity<OrderState, ProcessingStarted>
{
    public override async Task Execute(
        BehaviorContext<OrderState, ProcessingStarted> context,
        IBehavior<OrderState, ProcessingStarted> next)
    {
        await context.Publish(new OrderPaid
        {
            Id = context.Saga.CorrelationId
        });

        await next.Execute(context);
    }
}