using BuildingBlocks.Extensions;
using Ordering_Infrastructure.Extensions;
using Ordering.Core.Orders.Commands.CreateOrder;
using Ordering.Core.Services;
using ProductGrpc;

namespace Ordering.Api.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, bool isTest = false)
    {
        // Controllers
        services.AddControllers();
        
        services.AddOrderingInfrastructure(configuration);

        // MediatR و Pipeline Behaviors
        services.AddConfiguredMediatR(typeof(CreateOrderCommandHandler));

        // gRPC Client برای Product
        services.AddGrpcClientWithConfig<ProductProtoService.ProductProtoServiceClient>(
                                                                                        configuration, "GrpcSettings:CatalogUrl");
        services.AddScoped<IProductService, ProductGrpcService>();
        
        return services;
    }
}