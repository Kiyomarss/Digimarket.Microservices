using MassTransit;
using MassTransit.Metadata;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Ordering_Infrastructure.Extensions;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.Consumers;
using Ordering.Worker.DbContext;
using Ordering.Worker.StateMachines;
using Quartz;
using Serilog;

var builder = Host.CreateDefaultBuilder(args)
                  .ConfigureAppConfiguration((hostingContext, config) =>
                  {
                      config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables();
                  })
                  .ConfigureServices((hostContext, services) =>
                  {
                      var configuration = hostContext.Configuration;
                      var connectionString = configuration.GetConnectionString("Default") ?? 
                                             Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ?? 
                                             "host=localhost;user id=postgres;password=123;database=OrderingDb;";
                      var rabbitMqHost = configuration.GetSection("RabbitMQ:Host").Value ?? 
                                         Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
                      var rabbitMqUsername = configuration.GetSection("RabbitMQ:Username").Value ?? 
                                             Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest";
                      var rabbitMqPassword = configuration.GetSection("RabbitMQ:Password").Value ?? 
                                             Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";

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
                  })
                  .UseSerilog()
                  .Build();

await builder.RunAsync();