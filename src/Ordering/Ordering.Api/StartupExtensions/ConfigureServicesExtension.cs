using BuildingBlocks.Extensions;
using BuildingBlocks.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Ordering_Infrastructure.Data.DbContext;
using Ordering_Infrastructure.Repositories;
using Ordering.Application.RepositoryContracts;
using Ordering.Application.ServiceContracts;
using Ordering.Application.Services;
using ProductGrpc;

namespace Ordering.Api.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers
        services.AddControllers();
        
        services.AddDbContext<OrderingDbContext>(x =>
        {
            var connectionString = configuration.GetConnectionString("Default");

            x.UseNpgsql(connectionString, options =>
            {
                options.MigrationsAssembly(typeof(OrderingDbContext).Assembly.GetName().Name);
                options.MigrationsHistoryTable("__EFMigrationsHistory");

                options.EnableRetryOnFailure(5);
                options.MinBatchSize(1);
            });
        });

        // MediatR و Pipeline Behaviors
        services.AddConfiguredMediatR();

        // gRPC Client برای Product
        services.AddGrpcClientWithConfig<ProductProtoService.ProductProtoServiceClient>(
                                                                                        configuration, "GrpcSettings:CatalogUrl");
        services.AddScoped<IProductService, ProductGrpcService>();
        
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork<OrderingDbContext>>();
        
        return services;
    }
}