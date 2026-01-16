using MassTransit;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.DbContext;

namespace Ordering.Worker.Configurations;

//TODO: کاربرد کد زیر چیست؟
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