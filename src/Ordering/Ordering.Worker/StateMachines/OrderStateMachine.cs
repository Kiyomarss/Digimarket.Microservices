using MassTransit;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines.Activities.Initialize;
using Ordering.Worker.StateMachines.Activities.Inventory;
using Ordering.Worker.StateMachines.Activities.Payment;
using Ordering.Worker.StateMachines.Activities.Processing;
using Ordering.Worker.StateMachines.Events;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderInitiated, x => { x.CorrelateById(m => m.Message.Id); x.SelectId(m => m.Message.Id); });
            Event(() => PaymentCompleted, x => x.CorrelateById(m => m.Message.CorrelationId));
            Event(() => InventoryReduced, x => x.CorrelateById(m => m.Message.Id));
            Event(() => BasketRemoved, x => x.CorrelateById(m => m.Message.Id));
            Event(() => SendReminder, x => x.CorrelateById(m => m.Message.Id));
            Event(() => CancelOrder, x => x.CorrelateById(m => m.Message.Id));
            Event(() => ProcessingStarted, x => x.CorrelateById(m => m.Message.Id));

            //TODO: در زیر کد های مربوط به هر وضعیت شاید بهتر باشد به صورت جدا و متد وار نوشته شود در این صورت تست نویسی نیز می‌تواند ساده تر باشد
            Initially(
                      When(OrderInitiated)
                          .Activity(x => x.OfType<InitializeOrderActivity>())
                          .Activity(x => x.OfType<ScheduleOrderActivity>())
                          .TransitionTo(WaitingForPayment)
                     );
            
            During(WaitingForPayment,
                   When(PaymentCompleted)
                       .Activity(x => x.OfType<ValidatePaymentActivity>())
                       .Activity(x => x.OfType<CancelTimeoutsActivity>())
                       .TransitionTo(WaitingForProcessing)
                  );

            During(WaitingForPayment,
                   When(CancelOrder)
                       .Activity(x => x.OfType<ReleaseInventoryActivity>())
                       .TransitionTo(Cancelled)
                  );

            During(WaitingForProcessing,
                   When(ProcessingStarted)
                       .Activity(x => x.OfType<PublishOrderPaidActivity>())
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
        private Event<BasketRemoved> BasketRemoved { get; set; } = null!;
        public Event<SendReminder> SendReminder { get; private set; } = null!;
        public Event<CancelOrder> CancelOrder { get; private set; } = null!;
        public Event<ProcessingStarted> ProcessingStarted { get; private set; } = null!;
    }
}
