using MassTransit;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.DbContext;

namespace Ordering.Worker.Configurations;

public class OrdersStateDefinition :
    SagaDefinition<OrderState>
{
    protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator,
        ISagaConfigurator<OrderState> consumerConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(10, 50, 100, 1000, 1000, 1000, 1000, 1000));

        endpointConfigurator.UseEntityFrameworkOutbox<OrdersSagaDbContext>(context);
    }
}