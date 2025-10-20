using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Components;

namespace Basket.Api.StartupExtensions;

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

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        // Scoped Services
        services.AddScoped<OrderService, OrderService>();

        return services;
    }
}