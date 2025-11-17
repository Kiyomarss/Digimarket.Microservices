using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Ordering.ApiTests.Utilities;

public static class MassTransitTestExtensions
{
    public static void RemoveMassTransitForTests(this IServiceCollection services)
    {
        var descriptors = services.Where(x =>
                                             x.ImplementationType != null &&
                                             (
                                                 typeof(MassTransitHostedService).IsAssignableFrom(x.ImplementationType) ||
                                                 typeof(MassTransit.EntityFrameworkCoreIntegration.BusOutboxDeliveryService<>).IsAssignableFromGeneric(x.ImplementationType) ||
                                                 typeof(MassTransit.EntityFrameworkCoreIntegration.EntityFrameworkScopedBusContextProvider<,>).IsAssignableFromGeneric(x.ImplementationType)
                                             ) ||
                                             x.ServiceType == typeof(IBus) ||
                                             x.ServiceType == typeof(IBusControl) ||
                                             x.ServiceType == typeof(IBusDepot)
                                        ).ToList();

        foreach (var d in descriptors)
            services.Remove(d);

        // Fake endpoints
        services.AddSingleton<IPublishEndpoint, FakePublishEndpoint>();
        services.AddSingleton<ISendEndpointProvider, FakePublishEndpoint>();

        // Optional: fake IBus/IBusControl
        services.AddSingleton<IBus, FakeBus>();
        services.AddSingleton<IBusControl, FakeBus>();
    }
}