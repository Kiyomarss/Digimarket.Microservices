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
        public static IServiceCollection AddOrderingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // استخراج تنظیمات دیتابیس
            var connectionString = GetConfigValue(configuration, "ConnectionStrings:Default", 
                Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ?? 
                "host=localhost;user id=postgres;password=123;database=OrderingDb;");

            // استخراج تنظیمات RabbitMQ
            var rabbitMqHost = GetConfigValue(configuration, "RabbitMQ:Host", 
                Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost");
            var rabbitMqUsername = GetConfigValue(configuration, "RabbitMQ:Username", 
                Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest");
            var rabbitMqPassword = GetConfigValue(configuration, "RabbitMQ:Password", 
                Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest");

            // ثبت DbContext اصلی
            services.AddOrderingInfrastructure(configuration);

            // ثبت DbContext برای Saga
            services.AddDbContext<OrdersSagaDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgOptions =>
                {
                    npgOptions.MinBatchSize(1);
                    npgOptions.MigrationsAssembly(typeof(OrdersSagaDbContext).Assembly.GetName().Name);
                });
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
                    cfg.Host(rabbitMqHost, "/", h =>
                    {
                        h.Username(rabbitMqUsername);
                        h.Password(rabbitMqPassword);
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

        private static string GetConfigValue(IConfiguration configuration, string key, string defaultValue)
        {
            return configuration[key] ?? defaultValue;
        }
    }
}