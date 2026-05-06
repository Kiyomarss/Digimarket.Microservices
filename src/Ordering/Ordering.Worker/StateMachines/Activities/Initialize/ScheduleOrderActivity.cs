using BuildingBlocks.IntegrationEvents;
using MassTransit;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines.Activities.Common;
using Ordering.Worker.StateMachines.Contracts.Dtos;
using Ordering.Worker.StateMachines.Contracts.Events;

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

            var items = context.Message.Items.Select(x => new OrderItemDto(x.ProductId, x.Quantity));
            var cancel = await context.ScheduleSend(
                                                    DateTime.UtcNow.AddSeconds(5),
                                                    new CancelOrder(orderId, items));

            context.Saga.CancelScheduleTokenId = cancel.TokenId;

            await next.Execute(context);
        }
    }
}