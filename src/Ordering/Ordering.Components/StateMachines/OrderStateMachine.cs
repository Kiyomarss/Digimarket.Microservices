using MassTransit;
using Ordering.Components.Contracts;

namespace Ordering.Components.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderInitiated, x => x.CorrelateById(m => m.Message.Id));
            Event(() => PaymentCompleted, x => x.CorrelateById(m => m.Message.Id));
            Event(() => InventoryReduced, x => x.CorrelateById(m => m.Message.Id));
            Event(() => BasketRemoved, x => x.CorrelateById(m => m.Message.Id));
            Event(() => SendReminder, x => x.CorrelateById(m => m.Message.Id));
            Event(() => CancelOrder, x => x.CorrelateById(m => m.Message.Id));
            Event(() => ProcessingStarted, x => x.CorrelateById(m => m.Message.Id));

            Initially(
                When(OrderInitiated)
                    .Then(context =>
                    {
                        context.Saga.Date = context.Message.Date;
                        context.Saga.Customer = context.Message.Customer;
                    })
                    .ThenAsync(async context =>
                    {
                        var orderId = context.Saga.CorrelationId;

                        await Task.WhenAll(
                            context.Publish(new ReduceInventory(orderId)),
                            context.Publish(new RemoveBasket(orderId))
                        );
                        
                        var scheduledReminder = await context.ScheduleSend(
                            DateTime.UtcNow.AddMinutes(10),
                            new SendReminder(orderId)
                        );
                        context.Saga.ReminderScheduleTokenId = scheduledReminder.TokenId;

                        var scheduledCancel = await context.ScheduleSend(
                            DateTime.UtcNow.AddMinutes(15),
                            new CancelOrder(orderId)
                        );
                        context.Saga.CancelScheduleTokenId = scheduledCancel.TokenId;
                    })
                    .TransitionTo(WaitingForPayment)
            );
            
            During(WaitingForPayment,
                When(PaymentCompleted)
                    .Then(context =>
                    {
                        context.Saga.IsPaymentValidated = true;
                        context.Saga.PaymentReference = context.Message.TransactionId;
                    })
                    .ThenAsync(async context =>
                    {
                        if (context.Saga.ReminderScheduleTokenId != null && !context.Saga.IsReminderSent)
                        {
                            var scheduler = context.GetPayload<MessageSchedulerContext>();
                            await scheduler.CancelScheduledSend(QuartzSchedulerUri, context.Saga.ReminderScheduleTokenId.Value);
                            context.Saga.ReminderScheduleTokenId = null;
                        }

                        if (context.Saga.CancelScheduleTokenId != null)
                        {
                            var scheduler = context.GetPayload<MessageSchedulerContext>();
                            await scheduler.CancelScheduledSend(QuartzSchedulerUri, context.Saga.CancelScheduleTokenId.Value);
                            context.Saga.CancelScheduleTokenId = null;
                        }

                        await context.Publish(new OrderReadyToProcess(context.Saga.CorrelationId));
                        await context.Raise(ProcessingStarted);
                    })
                    .TransitionTo(WaitingForProcessing)
            );

            During(WaitingForPayment,
                When(SendReminder)
                    .ThenAsync(async context =>
                    {
                        await context.Publish(new SendSmsReminder(context.Saga.CorrelationId));
                        context.Saga.IsReminderSent = true;
                    })
            );

            During(WaitingForPayment,
                When(CancelOrder)
                    .ThenAsync(async context =>
                    {
                        await context.Publish(new ReleaseInventory(context.Saga.CorrelationId));
                        await context.Publish(new CancelOrderNotification(context.Saga.CorrelationId));
                    })
                    .TransitionTo(Cancelled)
            );

            During(WaitingForProcessing,
                When(InventoryReduced)
                    .Then(context => context.Saga.IsInventoryReduced = true)
                    .ThenAsync(CheckAndFinalize),

                When(BasketRemoved)
                    .Then(context => context.Saga.IsBasketRemoved = true)
                    .ThenAsync(CheckAndFinalize)
            );

            During(WaitingForProcessing,
                When(ProcessingStarted)
                    .TransitionTo(Processing)
            );
        }

        private static readonly Uri QuartzSchedulerUri = new Uri("queue:quartz");

        public State WaitingForPayment { get; private set; } = null!;
        public State WaitingForProcessing { get; private set; } = null!;
        public State Processing { get; private set; } = null!;
        public State Cancelled { get; private set; } = null!;

        public Event<OrderInitiated> OrderInitiated { get; private set; } = null!;
        public Event<PaymentCompleted> PaymentCompleted { get; private set; } = null!;
        public Event<InventoryReduced> InventoryReduced { get; private set; } = null!;
        public Event<BasketRemoved> BasketRemoved { get; private set; } = null!;
        public Event<SendReminder> SendReminder { get; private set; } = null!;
        public Event<CancelOrder> CancelOrder { get; private set; } = null!;
        public Event<OrderReadyToProcess> ProcessingStarted { get; private set; } = null!;

        private async Task CheckAndFinalize(BehaviorContext<OrderState> context)
        {
            var saga = context.Saga;
            if (saga.IsInventoryReduced && saga.IsBasketRemoved)
            {
                await context.Publish(new OrderReadyToProcess(saga.CorrelationId));
                await context.Raise(ProcessingStarted);
            }
        }
    }
}
