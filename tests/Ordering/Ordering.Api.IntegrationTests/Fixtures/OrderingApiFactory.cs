// tests/Ordering.Api.IntegrationTests/Fixtures/OrderingApiFactory.cs

using BuildingBlocks.Extensions;
using BuildingBlocks.UnitOfWork;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OrderGrpc;
using Ordering_Domain.Domain.RepositoryContracts;
using Ordering_Infrastructure.Data.DbContext;
using Ordering_Infrastructure.Data.Persistence;
using Ordering_Infrastructure.Extensions;
using Ordering_Infrastructure.Repositories;
using Ordering.Core.Orders.Commands.CreateOrder;
using Ordering.Core.Services;
using Respawn;
using Shared.TestFixtures;
using Testcontainers.PostgreSql;

namespace Ordering.Api.IntegrationTests.Fixtures;

public class OrderingApiFactory : IAsyncLifetime
{
    private readonly IContainer _rabbitMqContainer;
    private readonly IContainer _postgresContainer;
    private readonly ServiceProvider _serviceProvider;
    public IServiceProvider Services => _serviceProvider;
    public IBusControl Bus => _serviceProvider.GetRequiredService<IBusControl>();
    public OrderingDbContext DbContext => _serviceProvider.GetRequiredService<OrderingDbContext>();
    public Mock<IOrderRepository> MockOrderRepository { get; } = new();

    public OrderingApiFactory()
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

        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        var connectionString = $"Host=localhost;Port={_postgresContainer.GetMappedPublicPort(5432)};Database=OrderingDb;Username=postgres;Password=123;";
        services.AddDbContext<OrderingDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var productServiceMock = new ProductServiceMockBuilder().WithDefaultProducts().Build();

        services.AddScoped<IProductService>(sp => productServiceMock);
        
        services.AddConfiguredMediatR(typeof(CreateOrderCommandHandler));
        
        services.AddGrpcClient<OrderProtoService.OrderProtoServiceClient>(o =>
        {
            o.Address = new Uri("http://localhost");
        });
        
        services.AddMassTransit(x =>
        {
            x.AddMassTransitTestHarness();
            
            x.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.UsePostgres();
                o.UseBusOutbox();
            });
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host($"amqp://guest:guest@localhost:{_rabbitMqContainer.GetMappedPublicPort(5672)}");
                cfg.ConfigureEndpoints(context);
            });
        });

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        dbContext.Database.Migrate();
    }

    // این دو متد را دقیقاً با این نام و امضا داشته باش (public و async)
    public async Task InitializeAsync()
    {
        await _serviceProvider.GetRequiredService<IBusControl>().StartAsync();
    }
    
    public async Task StartAsync()
    {
        await Bus.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Bus.StopAsync();
        await _rabbitMqContainer.DisposeAsync();
        await _postgresContainer.DisposeAsync();
        await _serviceProvider.DisposeAsync();
        await DbContext.DisposeAsync();
    }
}