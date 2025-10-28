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
using Ordering.Worker.Configurations;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.Consumers;
using Ordering.Worker.DbContext;
using Ordering.Worker.StateMachines;

var builder = Host.CreateDefaultBuilder(args)
                  .ConfigureServices((hostContext, services) =>
                  {
                      services.AddDbContext<OrdersSagaDbContext>(x =>
                      {
                          var connectionString = hostContext.Configuration.GetConnectionString("Default");
                          x.UseNpgsql(connectionString, options =>
                          {
                              options.MinBatchSize(1);
                          });
                      });

                      services.AddOpenTelemetry().WithTracing(x =>
                      {
                          x.SetResourceBuilder(ResourceBuilder.CreateDefault()
                                                              .AddService("service")
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
                      
                      services.AddQuartz();
                      services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

                      services.AddMassTransit(x =>
                      {
                          x.AddEntityFrameworkOutbox<OrdersSagaDbContext>(o =>
                          {
                              o.UsePostgres();
                              o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                          });

                          x.SetKebabCaseEndpointNameFormatter();

                          x.AddConsumer<OrderStatusChangedConsumer>();
                          x.AddConsumer<OrderInitiatedConsumer>();
                          x.AddConsumer<OrderReadyToProcessConsumer>();
                          x.AddConsumer<SendOrderEmailConsumer>();
                          x.AddConsumer<ValidateOrdersConsumer, ValidateOrdeConsumerDefinition>();

                          x.AddSagaStateMachine<OrderStateMachine, OrderState, OrdersStateDefinition>()
                           .EntityFrameworkRepository(r =>
                           {
                               r.ExistingDbContext<OrdersSagaDbContext>();
                               r.UsePostgres();
                           });

                          x.AddQuartzConsumers();

                          x.UsingRabbitMq((context, cfg) =>
                          {
                              cfg.Host("rabbitmq://localhost");
                              cfg.UseMessageScheduler(new Uri("queue:quartz"));
                              cfg.ConfigureEndpoints(context);
                          });
                      });
                  })
                  .UseSerilog()
                  .Build();

await builder.RunAsync();
