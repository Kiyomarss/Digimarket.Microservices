using Microsoft.EntityFrameworkCore;
using Ordering.Worker.DbContext;
using MassTransit;
using MassTransit.Metadata;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Ordering_Infrastructure.Extensions;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.Consumers;
using Ordering.Worker.StateMachines;
using Quartz;

namespace Ordering.Worker.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrderingServices(this IServiceCollection services, IConfiguration configuration,  IHostEnvironment environment)
        {
            // استخراج تنظیمات دیتابیس
            var connectionString = configuration.GetConnectionString("Default");
            
            // ثبت DbContext اصلی
            services.AddOrderingInfrastructure(configuration, environment);

            // ثبت DbContext برای Saga
            services.AddDbContext<OrdersSagaDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            // ثبت MassTransit
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumers(typeof(OrderInitiatedConsumer).Assembly);
                x.AddConsumers(typeof(OrderStatusChangedConsumer).Assembly);
                x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ExistingDbContext<OrdersSagaDbContext>();
                        r.UsePostgres();
                    });
                x.AddQuartzConsumers();
                x.AddEntityFrameworkOutbox<OrdersSagaDbContext>(o =>
                {
                    o.QueryDelay = TimeSpan.FromSeconds(1);
                    o.UsePostgres();
                    o.UseBusOutbox();
                });
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    cfg.UseMessageScheduler(new Uri("queue:quartz"));
                    cfg.ConfigureEndpoints(context);
                });
            });

            // ثبت Quartz
            services.AddQuartz();
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            // ثبت OpenTelemetry
            services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService("Ordering.Worker")
                        .AddTelemetrySdk()
                        .AddEnvironmentVariableDetector())
                    .AddSource("MassTransit")
                    .AddJaegerExporter(o =>
                    {
                        o.AgentHost = HostMetadataCache.IsRunningInContainer ? "jaeger" : "localhost";
                        o.AgentPort = 6831;
                    });
            });

            return services;
        }
    }
}