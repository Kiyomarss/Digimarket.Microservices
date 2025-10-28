using MassTransit;
using Quartz;
using Serilog;
using System.Diagnostics;
using MassTransit.Metadata;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Ordering_Infrastructure.Data.DbContext;
using Ordering_Infrastructure.Repositories;
using Ordering_Domain.Domain.RepositoryContracts;
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

        // -------------------------
        // Ordering domain DbContext (Orders, OrderItems) - used by repositories
        // -------------------------
        services.AddDbContext<OrderingDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgOptions =>
            {
                // optional: migrations assembly for infrastructure project
                npgOptions.MigrationsAssembly(typeof(OrderingDbContext).Assembly.GetName().Name);
                npgOptions.MinBatchSize(1);
                npgOptions.EnableRetryOnFailure(5);
            });
        });

        // Register domain repositories / services (infrastructure implementations)
        services.AddScoped<IOrderRepository, OrderRepository>();

        // -------------------------
        // Orders Saga DbContext (Saga states) - dedicated DB context for saga persistence
        // -------------------------
        services.AddDbContext<OrdersSagaDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgOptions =>
            {
                npgOptions.MinBatchSize(1);
                // migrations assembly optional:
                npgOptions.MigrationsAssembly(typeof(OrdersSagaDbContext).Assembly.GetName().Name);
            });
        });

        // -------------------------
        // OpenTelemetry / Jaeger
        // -------------------------
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
                    o.MaxPayloadSizeInBytes = 4096;
                    o.ExportProcessorType = ExportProcessorType.Batch;
                    o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                    {
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 30000,
                        MaxExportBatchSize = 512,
                    };
                });
        });

        // -------------------------
        // Quartz
        // -------------------------
        services.AddQuartz();
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // -------------------------
        // MassTransit
        // -------------------------
        services.AddMassTransit(x =>
        {
            // Entity Framework Outbox for Saga DB (so saga publishes can be part of outbox)
            x.AddEntityFrameworkOutbox<OrdersSagaDbContext>(o =>
            {
                o.UsePostgres();
                o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
            });

            x.SetKebabCaseEndpointNameFormatter();

            // Register Consumers (from Worker assembly)
            x.AddConsumer<OrderStatusChangedConsumer>();
            x.AddConsumer<OrderInitiatedConsumer>();
            x.AddConsumer<OrderReadyToProcessConsumer>();
            x.AddConsumer<SendOrderEmailConsumer>();
            x.AddConsumer<ValidateOrdersConsumer, ValidateOrdeConsumerDefinition>();

            // Register Saga state machine (uses OrdersSagaDbContext)
            x.AddSagaStateMachine<OrderStateMachine, OrderState, OrdersStateDefinition>()
             .EntityFrameworkRepository(r =>
             {
                 // use the OrdersSagaDbContext that we registered above
                 r.ExistingDbContext<OrdersSagaDbContext>();
                 r.UsePostgres();
             });

            // Quartz consumers for scheduled messages
            x.AddQuartzConsumers();

            // Transport configuration (RabbitMQ)
            x.UsingRabbitMq((context, cfg) =>
            {
                // host can be read from config if you prefer
                cfg.Host("rabbitmq://localhost", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                // enable scheduler queue for Quartz
                cfg.UseMessageScheduler(new Uri("queue:quartz"));

                // configure endpoints for consumers/sagas automatically
                cfg.ConfigureEndpoints(context);
            });
        });

        // -------------------------
        // (Optional) Hosted services or other registrations...
        // -------------------------
        // e.g. services.AddHostedService<SomeHostedService>();

    })
    .UseSerilog()
    .Build();

await builder.RunAsync();