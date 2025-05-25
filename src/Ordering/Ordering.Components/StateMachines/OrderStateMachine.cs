using MassTransit;
using Ordering.Components.Contracts;

namespace Ordering.Components.StateMachines;

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => RegistrationSubmitted, x => x.CorrelateById(m => m.Message.RegistrationId));

        Initially(
            When(RegistrationSubmitted)
                .Then(context =>
                {
                    context.Saga.Date = context.Message.RegistrationDate;
                    context.Saga.Customer = context.Message.Customer;
                })
                .TransitionTo(Registered)
                .Publish(context => new SendRegistrationEmail
                {
                    RegistrationId = context.Saga.CorrelationId,
                    RegistrationDate = context.Saga.Date,
                    Customer = context.Saga.Customer
                })
                .Publish(context => new AddEventAttendee
                {
                    RegistrationId = context.Saga.CorrelationId,
                    Customer = context.Saga.Customer
                })
        );
    }

    //
    // ReSharper disable MemberCanBePrivate.Global
    public State Registered { get; } = null!;
    public Event<OrderSubmitted> RegistrationSubmitted { get; } = null!;
}