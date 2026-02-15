using MassTransit;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines.Events;
using Ordering.Worker.StateMachines.Activities.Common;

namespace Ordering.Worker.StateMachines.Activities.Payment
{
    public class ValidatePaymentActivity :
        BaseActivity<OrderState, PaymentCompleted>
    {
        public override Task Execute(
            BehaviorContext<OrderState, PaymentCompleted> context,
            IBehavior<OrderState, PaymentCompleted> next)
        {
            context.Saga.IsPaymentValidated = true;
            context.Saga.PaymentReference = context.Message.CorrelationId;

            return next.Execute(context);
        }
    }
}