using BuildingBlocks.Extensions;
using FluentValidation;
using Ordering_Infrastructure.Extensions;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Application.Services;
using ProductGrpc;

namespace Ordering.Api.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment, bool isTest = false)
    {
        // Controllers
        services.AddControllers();
        
        services.AddOrderingInfrastructure(configuration, environment);

        // MediatR و Pipeline Behaviors
        services.AddConfiguredMediatR(typeof(CreateOrderCommandHandler));
        
        //TODO: در پروژه زیر از دستور زیر استفاده نشده و کد به درستی کار می‌کرد. این مشکل با توجه به نیازی که در تست Worker بود به صورت زیر نوشته شد. در صورت اصلاح می‌توان بخش زیر و ورودی بخش بالا را اصلاح نمود
        //D:\Repository\aspnetcore-microservices
        services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>();

        // gRPC Client برای Product
        services.AddGrpcClientWithConfig<ProductProtoService.ProductProtoServiceClient>(
                                                                                        configuration, "GrpcSettings:CatalogUrl");
        services.AddScoped<IProductService, ProductGrpcService>();
        
        return services;
    }
}