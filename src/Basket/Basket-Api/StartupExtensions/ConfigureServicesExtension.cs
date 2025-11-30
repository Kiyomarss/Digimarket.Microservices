using System.Reflection;
using Basket_Application.Orders;
using Basket_Application.Orders.Commands.CreateOrder;
using Basket.Domain.RepositoryContracts;
using Basket.Infrastructure.Data.DbContext;
using Basket.Infrastructure.Repositories;
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

        // MassTransit + Outbox
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<BasketDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((_, cfg) =>
            {
                cfg.AutoStart = true;
            });
        });

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
        services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderHandler).Assembly));
        
        // Scoped Services
        services.AddScoped<IBasketRepository, BasketRepository>();
        services.Decorate<IBasketRepository, CachedBasketRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork<BasketDbContext>>();

        services.AddConfiguredMediatR(typeof(CreateOrderHandler));

        services.AddGrpcClientWithConfig<OrderProtoService.OrderProtoServiceClient>(configuration, "GrpcSettings:OrderUrl");
        
        return services;
    }
}