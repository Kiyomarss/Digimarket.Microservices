using MassTransit;

namespace Ordering.Components.Consumers;

public class ValidateOrdeConsumerDefinition :
    ConsumerDefinition<ValidateOrdersConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ValidateOrdersConsumer> consumerConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(10, 50, 100, 1000, 1000, 1000, 1000, 1000));

        endpointConfigurator.UseEntityFrameworkOutbox<OrderDbContext>(context);
    }
}