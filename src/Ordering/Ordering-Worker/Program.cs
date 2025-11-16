using MassTransit;
using Quartz;
using Serilog;
using MassTransit.Metadata;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Ordering_Infrastructure.Extensions;
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
        //services.AddHostedService<RecreateDatabaseHostedService<OrdersSagaDbContext>>();
        //services.AddHostedService<RecreateDatabaseHostedService<OrderingDbContext>>();

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
            x.AddConsumers(typeof(OrderInitiatedConsumer).Assembly);
            x.AddSagaStateMachine<OrderStateMachine, OrderState>()
             .EntityFrameworkRepository(r =>
             {
                 r.ExistingDbContext<OrdersSagaDbContext>();
                 r.UsePostgres();
             });
            x.AddQuartzConsumers();
            x.AddEntityFrameworkOutbox<OrdersSagaDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1); // تنظیم تأخیر برای بررسی Outbox
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
    })
    .UseSerilog()
    .Build();

await builder.RunAsync();