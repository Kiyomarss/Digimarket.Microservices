using MassTransit;
using Ordering.Components.Contracts;

namespace Ordering.Components.StateMachines;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderInitiated, x => x.CorrelateById(m => m.Message.Id));
        Event(() => PaymentCompleted, x => x.CorrelateById(m => m.Message.Id));
        Event(() => InventoryReduced, x => x.CorrelateById(m => m.Message.Id));
        Event(() => BasketRemoved, x => x.CorrelateById(m => m.Message.Id));

        Initially(
            When(OrderInitiated)
                .Then(context =>
                {
                    context.Saga.Date = context.Message.Date;
                    context.Saga.Customer = context.Message.Customer;
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
                    var orderId = context.Saga.CorrelationId;
                    await Task.WhenAll(
                        context.Publish(new ReduceInventory(orderId)),
                        context.Publish(new RemoveBasket(orderId))
                    );
                })
                .TransitionTo(WaitingForProcessing)
        );

        During(WaitingForProcessing,
               When(ProcessingStarted)
                   .TransitionTo(Processing)
              );
        
        During(WaitingForProcessing,
            When(InventoryReduced)
                .Then(context => context.Saga.IsInventoryReduced = true)
                .ThenAsync(CheckAndFinalize),

            When(BasketRemoved)
                .Then(context => context.Saga.IsBasketRemoved = true)
                .ThenAsync(CheckAndFinalize)
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
