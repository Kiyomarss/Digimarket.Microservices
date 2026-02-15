using MassTransit;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines.Activities.Common;
using Ordering.Worker.StateMachines.Events;

namespace Ordering.Worker.StateMachines.Activities.Payment
{
    public class CancelTimeoutsActivity :
        BaseActivity<OrderState, PaymentCompleted>
    {
        private static readonly Uri QuartzSchedulerUri = new("queue:quartz");

        public override async Task Execute(
            BehaviorContext<OrderState, PaymentCompleted> context,
            IBehavior<OrderState, PaymentCompleted> next)
        {
            var scheduler = context.GetPayload<MessageSchedulerContext>();

            if (context.Saga.ReminderScheduleTokenId != null && !context.Saga.IsReminderSent)
            {
                await scheduler.CancelScheduledSend(
                                                    QuartzSchedulerUri,
                                                    context.Saga.ReminderScheduleTokenId.Value);

                context.Saga.ReminderScheduleTokenId = null;
            }

            if (context.Saga.CancelScheduleTokenId != null)
            {
                await scheduler.CancelScheduledSend(
                                                    QuartzSchedulerUri,
                                                    context.Saga.CancelScheduleTokenId.Value);

                context.Saga.CancelScheduleTokenId = null;
            }

            await context.Publish(new ProcessingStarted(context.Saga.CorrelationId));

            await next.Execute(context);
        }
    }
}