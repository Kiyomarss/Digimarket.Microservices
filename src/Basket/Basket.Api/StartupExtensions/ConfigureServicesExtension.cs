using System.Reflection;
using Basket_Application.Basket.Consumers;
using Basket_Application.Orders.Commands.CreateOrder;
using Basket_Application.RepositoryContracts;
using Basket.Infrastructure.Data.DbContext;
using Basket.Infrastructure.Repositories;
using BuildingBlocks.Caching;
using BuildingBlocks.Configurations;
using BuildingBlocks.Extensions;
using BuildingBlocks.UnitOfWork;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderGrpc;

namespace Basket.Api.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers
        services.AddControllers();

        // Database Context
        services.AddDbContext<BasketDbContext>(x =>
        {
            var connectionString = configuration.GetConnectionString("Default");

            x.UseNpgsql(connectionString, options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                options.MigrationsHistoryTable($"__{nameof(BasketDbContext)}");

                options.EnableRetryOnFailure(5);
                options.MinBatchSize(1);
            });
        });

        // Cache
        services.AddCaching(configuration);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderHandler).Assembly));
        
        // Scoped Services
        services.AddScoped<IBasketRepository, BasketRepository>();
        services.Decorate<IBasketRepository, CachedBasketRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork<BasketDbContext>>();

        services.AddConfiguredMediatR();

        services.AddGrpcClientWithConfig<OrderProtoService.OrderProtoServiceClient>(configuration, "GrpcSettings:OrderUrl");
        
        return services;
    }
}