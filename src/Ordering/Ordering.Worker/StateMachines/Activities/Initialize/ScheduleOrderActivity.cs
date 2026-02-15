using MassTransit;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines.Activities.Common;
using Ordering.Worker.StateMachines.Events;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.StateMachines.Activities.Initialize
{
    public class ScheduleOrderActivity :
        BaseActivity<OrderState, OrderInitiated>
    {
        public override async Task Execute(
            BehaviorContext<OrderState, OrderInitiated> context,
            IBehavior<OrderState, OrderInitiated> next)
        {
            var orderId = context.Saga.CorrelationId;

            var reminder = await context.ScheduleSend(
                                                      DateTime.UtcNow.AddMinutes(1),
                                                      new SendReminder(orderId));

            context.Saga.ReminderScheduleTokenId = reminder.TokenId;

            var cancel = await context.ScheduleSend(
                                                    DateTime.UtcNow.AddMinutes(2),
                                                    new CancelOrder(orderId));

            context.Saga.CancelScheduleTokenId = cancel.TokenId;

            await context.Publish(new OrderPaid
            {
                Id = orderId
            });

            await next.Execute(context);
        }
    }
}