using System.Reflection;
using BuildingBlocks.Extensions;
using Microsoft.EntityFrameworkCore;
using Ordering_Domain.Domain.RepositoryContracts;
using Ordering_Infrastructure.Data.DbContext;
using Ordering_Infrastructure.Repositories;
using ProductGrpc;

namespace Ordering.Api.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers
        services.AddControllers();

        // Database Context
        services.AddDbContext<OrderingDbContext>(x =>
        {
            var connectionString = configuration.GetConnectionString("Default");

            x.UseNpgsql(connectionString, options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                options.MigrationsHistoryTable($"__{nameof(OrderingDbContext)}");

                options.EnableRetryOnFailure(5);
                options.MinBatchSize(1);
            });
        });

        // Scoped Services
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        services.AddGrpcClientWithConfig<ProductProtoService.ProductProtoServiceClient>(configuration, "GrpcSettings:CatalogUrl");

        return services;
    }
}