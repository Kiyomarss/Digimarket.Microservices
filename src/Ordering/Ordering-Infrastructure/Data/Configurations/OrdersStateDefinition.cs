using MassTransit;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Components.StateMachines;

namespace Ordering_Infrastructure.Data.Configurations;

public class OrdersStateDefinition :
    SagaDefinition<OrderState>
{
    protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator,
        ISagaConfigurator<OrderState> consumerConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(10, 50, 100, 1000, 1000, 1000, 1000, 1000));

        endpointConfigurator.UseEntityFrameworkOutbox<OrderingDbContext>(context);
    }
}