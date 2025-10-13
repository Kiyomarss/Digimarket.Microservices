using MassTransit;
using Ordering.Components.Contracts;

namespace Ordering.Components.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        private static readonly Uri QuartzSchedulerUri = new("queue:quartz");

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderInitiated, x => x.CorrelateById(m => m.Message.Id));
            Event(() => PaymentCompleted, x => x.CorrelateById(m => m.Message.CorrelationId));
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
                            DateTime.UtcNow.AddMinutes(1),
                            new SendReminder(orderId)
                        );
                        context.Saga.ReminderScheduleTokenId = scheduledReminder.TokenId;

                        var scheduledCancel = await context.ScheduleSend(
                            DateTime.UtcNow.AddMinutes(2),
                            new CancelOrder(orderId)
                        );
                        context.Saga.CancelScheduleTokenId = scheduledCancel.TokenId;
                        await context.Publish(new OrderStatusChanged
                        {
                            Id = context.Saga.CorrelationId,
                            OrderState = WaitingForPayment.Name
                        });
                    })
                    .TransitionTo(WaitingForPayment)
            );
            
            During(WaitingForPayment,
                When(PaymentCompleted)
                    .Then(context =>
                    {
                        context.Saga.IsPaymentValidated = true;
                        context.Saga.PaymentReference = context.Message.CorrelationId;
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

                        await context.Publish(new ProcessingStarted(context.Saga.CorrelationId));
                    })
                    .TransitionTo(WaitingForProcessing)
            );

            During(WaitingForPayment,
                When(CancelOrder)
                    .ThenAsync(async context =>
                    {
                        await context.Publish(new ReleaseInventory(context.Saga.CorrelationId));
                    })
                    .TransitionTo(Cancelled)
            );

            During(WaitingForProcessing,
                   When(ProcessingStarted)
                       .ThenAsync(async context =>
                       {
                           await context.Publish(new OrderStatusChanged
                           {
                               Id = context.Saga.CorrelationId,
                               OrderState = Processing.Name
                           });
                       })
                       .TransitionTo(Processing)
                  );
        }
        
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
        public Event<ProcessingStarted> ProcessingStarted { get; private set; } = null!;
    }
}
