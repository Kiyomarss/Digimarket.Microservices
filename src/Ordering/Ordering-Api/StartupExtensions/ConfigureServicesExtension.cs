using BuildingBlocks.Extensions;
using Ordering_Infrastructure.Extensions;
using ProductGrpc;

namespace Ordering.Api.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers
        services.AddControllers();

        services.AddOrderingInfrastructure(configuration);
        
        services.AddGrpcClientWithConfig<ProductProtoService.ProductProtoServiceClient>(configuration, "GrpcSettings:CatalogUrl");

        return services;
    }
}