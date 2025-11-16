using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.DbContext;
using Ordering.Worker.StateMachines;
using Ordering_Domain.Domain.RepositoryContracts;
using Quartz;

namespace Ordering.Worker.IntegrationTests.Fixtures
{
    public class OrderStateMachineFixture : IAsyncDisposable
    {
        private readonly IContainer _rabbitMqContainer;
        private readonly IContainer _postgresContainer;
        private readonly ServiceProvider _serviceProvider;
        public IBusControl Bus { get; }
        public OrdersSagaDbContext DbContext { get; }
        public Mock<IOrderRepository> MockOrderRepository { get; }

        public OrderStateMachineFixture()
        {
            // راه‌اندازی RabbitMQ با Testcontainers
            _rabbitMqContainer = new ContainerBuilder()
                .WithImage("rabbitmq:3-management")
                .WithPortBinding(5672, true)
                .WithPortBinding(15672, true)
                .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
                .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5672))
                .Build();
            _rabbitMqContainer.StartAsync().GetAwaiter().GetResult();

            // راه‌اندازی PostgreSQL با Testcontainers
            _postgresContainer = new ContainerBuilder()
                .WithImage("postgres:16")
                .WithPortBinding(5432, true)
                .WithEnvironment("POSTGRES_USER", "postgres")
                .WithEnvironment("POSTGRES_PASSWORD", "123")
                .WithEnvironment("POSTGRES_DB", "OrderingDb")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
                .Build();
            _postgresContainer.StartAsync().GetAwaiter().GetResult();

            Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", $"Host=localhost;Port={_postgresContainer.GetMappedPublicPort(5432)};Database=OrderingDb;Username=postgres;Password=123;");
            Environment.SetEnvironmentVariable("RABBITMQ_HOST", $"localhost:{_rabbitMqContainer.GetMappedPublicPort(5672)}");
            
            // تنظیم ServiceCollection
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            var postgresConnectionString = $"Host=localhost;Port={_postgresContainer.GetMappedPublicPort(5432)};Database=OrderingDb;Username=postgres;Password=123;";
            services.AddDbContext<OrdersSagaDbContext>(options => options.UseNpgsql(postgresConnectionString));
            MockOrderRepository = new Mock<IOrderRepository>();
            services.AddSingleton(MockOrderRepository.Object);
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumers(typeof(Ordering.Worker.Consumers.OrderInitiatedConsumer).Assembly);
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
                    cfg.Host($"amqp://guest:guest@localhost:{_rabbitMqContainer.GetMappedPublicPort(5672)}");
                    cfg.UseMessageScheduler(new Uri("queue:quartz"));
                    cfg.ConfigureEndpoints(context);
                });
            });
            services.AddQuartz();
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            _serviceProvider = services.BuildServiceProvider();
            Bus = _serviceProvider.GetRequiredService<IBusControl>();
            DbContext = _serviceProvider.GetRequiredService<OrdersSagaDbContext>();
            DbContext.Database.Migrate();
        }

        public async Task StartAsync()
        {
            await Bus.StartAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await Bus.StopAsync();
            await _rabbitMqContainer.DisposeAsync();
            await _postgresContainer.DisposeAsync();
            await _serviceProvider.DisposeAsync();
            await DbContext.DisposeAsync();
        }
    }
}