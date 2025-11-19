// tests/Ordering.Application.IntegrationTests/Fixtures/OrderingIntegrationFixture.cs

using BuildingBlocks.Extensions;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Ordering_Infrastructure.Data.DbContext;
using Ordering_Infrastructure.Extensions;
using Ordering.Core.Orders.Commands.CreateOrder;
using Ordering_Domain.Domain.RepositoryContracts;
using BuildingBlocks.UnitOfWork;
using Ordering_Infrastructure.Data.Persistence;
using Ordering_Infrastructure.Repositories;
using Ordering.Core.Services;
using ProductGrpc;
using Quartz;
using Respawn;
using Shared.TestFixtures;

namespace Ordering.Application.IntegrationTests.Fixtures;

public class OrderingIntegrationFixture : IAsyncLifetime
{
    private readonly IContainer _postgresContainer;
    private readonly ServiceProvider _serviceProvider;
    public IServiceProvider Services => _serviceProvider;
    public IBusControl Bus => _serviceProvider.GetRequiredService<IBusControl>();
    public OrderingDbContext DbContext => _serviceProvider.GetRequiredService<OrderingDbContext>();
    public Mock<IOrderRepository> MockOrderRepository { get; } = new();

    public OrderingIntegrationFixture()
    {
        _postgresContainer = new ContainerBuilder()
            .WithImage("postgres:16")
            .WithPortBinding(5432, true)
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", "123")
            .WithEnvironment("POSTGRES_DB", "OrderingDb")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
            .Build();

        _postgresContainer.StartAsync().GetAwaiter().GetResult();

        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING",
            $"Host=localhost;Port={_postgresContainer.GetMappedPublicPort(5432)};Database=OrderingDb;Username=postgres;Password=123;");

        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        var connectionString = $"Host=localhost;Port={_postgresContainer.GetMappedPublicPort(5432)};Database=OrderingDb;Username=postgres;Password=123;";
        services.AddDbContext<OrderingDbContext>(options => options.UseNpgsql(connectionString));

        //services.AddSingleton(MockOrderRepository.Object);
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var productServiceMock = new ProductServiceMockBuilder().WithDefaultProducts().Build();

        services.AddScoped<IProductService>(sp => productServiceMock);
        
        services.AddConfiguredMediatR(typeof(CreateOrderCommandHandler));
        
        services.AddMassTransit(x =>
        {
            x.AddMassTransitTestHarness();
            
            x.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
        
        _serviceProvider = services.BuildServiceProvider();

        // Migrationها
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        dbContext.Database.Migrate();
    }

    // این دو متد را دقیقاً با این نام و امضا داشته باش (public و async)
    public async Task InitializeAsync()
    {
        await _serviceProvider.GetRequiredService<IBusControl>().StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _serviceProvider.GetRequiredService<IBusControl>().StopAsync();
        await _postgresContainer.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }
}