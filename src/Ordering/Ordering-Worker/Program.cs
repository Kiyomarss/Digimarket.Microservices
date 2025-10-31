using MassTransit;
using Quartz;
using Serilog;
using System.Diagnostics;
using MassTransit.Metadata;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Ordering_Infrastructure.Extensions;
using Ordering.Api;
using Ordering.Worker.Configurations;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.Consumers;
using Ordering.Worker.DbContext;
using Ordering.Worker.StateMachines;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;
        var connectionString = configuration.GetConnectionString("Default");

        // DbContext اصلی پروژه Ordering
        services.AddOrderingInfrastructure(configuration);

        // DbContext مخصوص Saga (state persistence)
        services.AddDbContext<OrdersSagaDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgOptions =>
            {
                npgOptions.MinBatchSize(1);
                npgOptions.MigrationsAssembly(typeof(OrdersSagaDbContext).Assembly.GetName().Name);
            });
        });

        // برای ساخت خودکار دیتابیس در حالت Dev (اختیاری)
        services.AddHostedService<RecreateDatabaseHostedService<OrdersSagaDbContext>>();

        // Telemetry (Jaeger / OpenTelemetry)
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

        // Quartz
        services.AddQuartz();
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // MassTransit configuration
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            // ثبت تمام consumerها از اسمبلی Worker
            x.AddConsumers(typeof(OrderInitiatedConsumer).Assembly);

            // ثبت Saga State Machine
            x.AddSagaStateMachine<OrderStateMachine, OrderState>()
             .EntityFrameworkRepository(r =>
             {
                 r.ExistingDbContext<OrdersSagaDbContext>();
                 r.UsePostgres();
             });

            // Quartz
            x.AddQuartzConsumers();

            // RabbitMQ
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                // فعال کردن Scheduler برای Quartz
                cfg.UseMessageScheduler(new Uri("queue:quartz"));

                // 🔹 در اینجا Outbox در سطح transport فعال می‌شود
                cfg.UseInMemoryOutbox(context);

                // ثبت خودکار endpointها
                cfg.ConfigureEndpoints(context);
            });
        });
    })
    .UseSerilog()
    .Build();

await builder.RunAsync();