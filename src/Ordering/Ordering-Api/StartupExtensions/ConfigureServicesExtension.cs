using BuildingBlocks.Extensions;
using Ordering_Infrastructure.Extensions;
using Ordering.Core.Orders.Commands.CreateOrder;
using ProductGrpc;

namespace Ordering.Api.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers
        services.AddControllers();

        services.AddOrderingInfrastructure(configuration);
        
        services.AddConfiguredMediatR(typeof(CreateOrderCommandHandler));
        services.AddGrpcClientWithConfig<ProductProtoService.ProductProtoServiceClient>(configuration, "GrpcSettings:CatalogUrl");

        return services;
    }
}