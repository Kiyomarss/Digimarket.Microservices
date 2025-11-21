// tests/Ordering.Api.IntegrationTests/Fixtures/OrderingApiFactory.cs

using BuildingBlocks.Extensions;
using BuildingBlocks.UnitOfWork;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Grpc.Net.Client;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
using ProductGrpc;
using Quartz;
using Respawn;
using Shared;
using Shared.TestFixtures;
using Testcontainers.PostgreSql;

namespace Ordering.Api.IntegrationTests.Fixtures;

public class OrderingApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IContainer _rabbitMqContainer;
    private readonly IContainer _postgresContainer;

    // public properties (tests expect to access Services, Bus, DbContext)
    // WebApplicationFactory already exposes "Services", so tests can use `Fixture.Services`
    public IBusControl Bus => Services.GetRequiredService<IBusControl>();
    public OrderingDbContext DbContext => Services.CreateScope().ServiceProvider.GetRequiredService<OrderingDbContext>();
    public Mock<IOrderRepository> MockOrderRepository { get; } = new();

    public OrderingApiFactory()
    {
        _rabbitMqContainer = TestContainerFactory.CreateRabbitMqContainer();
        _rabbitMqContainer.StartAsync().GetAwaiter().GetResult();

        _postgresContainer = TestContainerFactory.CreatePostgresContainer();
        _postgresContainer.StartAsync().GetAwaiter().GetResult();

        // Make ASPNETCORE_ENVIRONMENT available early (keeps behavior consistent)
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "IntegrationTest");

        // Set connection strings environment so ConfigureWebHost can read them
        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING",
            $"Host=localhost;Port={_postgresContainer.GetMappedPublicPort(5432)};Database=OrderingDb;Username=postgres;Password=123;");
        Environment.SetEnvironmentVariable("RABBITMQ_HOST",
            $"localhost:{_rabbitMqContainer.GetMappedPublicPort(5672)}");

        // Force creation of the host now so Services is available to tests immediately.
        // CreateDefaultClient triggers the host to build and run (in-memory test server).
        // We don't keep the HttpClient here — we just ensure host creation now.
        // (This will call ConfigureWebHost below.)
        _ = this.CreateDefaultClient();
        
        // After the host is built, run migrations once
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        db.Database.Migrate();
    }

    // Override ConfigureWebHost to register/override services inside the factory's host
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");

        builder.ConfigureServices(services =>
        {
            // ----- Register DB context using connection string from env (keeps parity with your original) -----
            // Remove any existing OrderingDbContext registration (if present) and re-register.
            services.RemoveAll<OrderingDbContext>();
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ??
                                   "Host=localhost;Database=OrderingDb;Username=postgres;Password=123;";
            services.AddDbContext<OrderingDbContext>(options => options.UseNpgsql(connectionString));

            // ----- Repository & UnitOfWork -----
            services.RemoveAll<IOrderRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // ----- Replace real Product gRPC client/service with a mock IProductService -----
            services.RemoveAll<ProductProtoService.ProductProtoServiceClient>();
            services.RemoveAll<IProductService>();
            var productServiceMock = new ProductServiceMockBuilder().WithDefaultProducts().Build();
            services.AddSingleton(productServiceMock);
        });
    }

    // IAsyncLifetime implementations: start/stop any background things (bus) when tests run
    public async Task InitializeAsync()
    {
        // Ensure bus is started before tests run
        var bus = Services.GetRequiredService<IBusControl>();
        await bus.StartAsync();
    }

    public async Task StartAsync()
    {
        var bus = Services.GetRequiredService<IBusControl>();
        await bus.StartAsync();
    }

    // Dispose: stop bus and containers — DO NOT attempt to resolve services after disposal.
    public async Task DisposeAsync()
    {
        try
        {
            // stop the bus if available
            var provider = Services;
            if (provider != null && provider.IsServiceRegistered<IBusControl>())
            {
                var bus = provider.GetRequiredService<IBusControl>();
                await bus.StopAsync();
            }
        }
        catch
        {
            // swallow — test teardown should be best-effort
        }

        // Dispose containers
        await _rabbitMqContainer.DisposeAsync();
        await _postgresContainer.DisposeAsync();

        // Dispose the factory host (base) so it cleans up server and services properly
        await base.DisposeAsync();
    }
}