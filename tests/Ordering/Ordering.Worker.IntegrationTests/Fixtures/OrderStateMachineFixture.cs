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
            _rabbitMqContainer.StartAsync().GetAwaiter().GetResult(); // راه‌اندازی همگام کانتینر

            // راه‌اندازی PostgreSQL با Testcontainers
            _postgresContainer = new ContainerBuilder()
                .WithImage("postgres:16")
                .WithPortBinding(5432, true)
                .WithEnvironment("POSTGRES_USER", "postgres")
                .WithEnvironment("POSTGRES_PASSWORD", "123")
                .WithEnvironment("POSTGRES_DB", "OrderingDb")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
                .Build();
            _postgresContainer.StartAsync().GetAwaiter().GetResult(); // راه‌اندازی همگام کانتینر

            // تنظیم ServiceCollection
            var services = new ServiceCollection();

            // افزودن ILoggerFactory
            services.AddLogging(builder =>
            {
                builder.AddConsole(); // استفاده از Console Logger به عنوان فراهم‌کننده
                builder.SetMinimumLevel(LogLevel.Information); // تنظیم سطح لاگ (اختیاری)
            });

            // تنظیم DbContext
            var postgresConnectionString = $"Host=localhost;Port={_postgresContainer.GetMappedPublicPort(5432)};Database=OrderingDb;Username=postgres;Password=123;";
            services.AddDbContext<OrdersSagaDbContext>(options =>
            {
                options.UseNpgsql(postgresConnectionString, npgOptions =>
                {
                    npgOptions.MinBatchSize(1);
                    npgOptions.MigrationsAssembly(typeof(OrdersSagaDbContext).Assembly.GetName().Name);
                });
            });

            // Mock کردن IOrderRepository
            MockOrderRepository = new Mock<IOrderRepository>();
            services.AddSingleton(MockOrderRepository.Object);

            // تنظیم MassTransit با RabbitMQ
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
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host($"amqp://guest:guest@localhost:{_rabbitMqContainer.GetMappedPublicPort(5672)}");
                    cfg.UseMessageScheduler(new Uri("queue:quartz"));
                    cfg.UseInMemoryOutbox(context);
                    cfg.ConfigureEndpoints(context);
                });
            });

            // Quartz
            services.AddQuartz();
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            _serviceProvider = services.BuildServiceProvider();

            // دریافت Bus و DbContext
            Bus = _serviceProvider.GetRequiredService<IBusControl>();
            DbContext = _serviceProvider.GetRequiredService<OrdersSagaDbContext>();

            // اعمال migrations
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