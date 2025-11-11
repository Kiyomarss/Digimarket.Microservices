using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Ordering.ApiTests.Utilities;

public static class MassTransitTestExtensions
{
    public static void RemoveMassTransitForTests(this IServiceCollection services)
    {
        var descriptors = services.Where(x =>
                                             x.ImplementationType == typeof(MassTransitHostedService) ||
                                             x.ServiceType == typeof(IBus) ||
                                             x.ServiceType == typeof(IBusControl) ||
                                             x.ServiceType == typeof(IBusDepot)
                                        ).ToList();

        foreach (var d in descriptors)
            services.Remove(d);

        services.AddSingleton<IPublishEndpoint, FakePublishEndpoint>();
        services.AddSingleton<ISendEndpointProvider, FakePublishEndpoint>();
    }
}