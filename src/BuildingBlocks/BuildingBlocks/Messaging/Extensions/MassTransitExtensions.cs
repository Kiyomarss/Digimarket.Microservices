using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Messaging.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddConfiguredMassTransit<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly consumerAssembly)
        where TDbContext : DbContext
    {
        services.AddMassTransit(x =>
        {
            // Auto register consumers
            x.AddConsumers(consumerAssembly);

            // Outbox (exactly-once message publishing)
            x.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.UsePostgres();
                o.UseBusOutbox();

                o.DuplicateDetectionWindow = TimeSpan.FromMinutes(30);
            });

            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "localhost";
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // -------------------------
                // PERFORMANCE
                // -------------------------

                cfg.PrefetchCount = 32;

                cfg.UseConcurrencyLimit(16);

                // -------------------------
                // RETRY POLICY
                // -------------------------

                cfg.UseMessageRetry(r =>
                {
                    r.Exponential(
                        retryLimit: 5,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(5));
                });

                // -------------------------
                // CIRCUIT BREAKER
                // -------------------------

                cfg.UseCircuitBreaker(cb =>
                {
                    cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                    cb.TripThreshold = 15;
                    cb.ActiveThreshold = 10;
                    cb.ResetInterval = TimeSpan.FromMinutes(5);
                });

                // -------------------------
                // DEAD LETTER / ERROR QUEUE
                // -------------------------

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}