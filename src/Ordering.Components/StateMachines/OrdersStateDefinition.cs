using MassTransit;

namespace Ordering.Components.StateMachines;

public class OrdersStateDefinition :
    SagaDefinition<OrdersState>
{
    protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator,
        ISagaConfigurator<OrdersState> consumerConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(10, 50, 100, 1000, 1000, 1000, 1000, 1000));

        endpointConfigurator.UseEntityFrameworkOutbox<OrderDbContext>(context);
    }
}