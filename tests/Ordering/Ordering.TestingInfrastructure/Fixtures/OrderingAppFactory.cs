// tests/Ordering.Api.IntegrationTests/Fixtures/OrderingApiFactory.cs

using BuildingBlocks.UnitOfWork;
using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Ordering_Domain.Domain.RepositoryContracts;
using Ordering_Infrastructure.Data.DbContext;
using Ordering_Infrastructure.Data.Persistence;
using Ordering_Infrastructure.Repositories;
using Ordering.Api;
using Ordering.Application.Services;
using ProductGrpc;
using Shared;
using Shared.TestFixtures;
using Xunit;

namespace Ordering.TestingInfrastructure.Fixtures;

public class OrderingAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IContainer _rabbitMqContainer;
    private readonly IContainer _postgresContainer;

    // public properties (tests expect to access Services, Bus, DbContext)
    // WebApplicationFactory already exposes "Services", so tests can use `Fixture.Services`
    public IBusControl Bus => Services.GetRequiredService<IBusControl>();
    public OrderingDbContext DbContext => Services.CreateScope().ServiceProvider.GetRequiredService<OrderingDbContext>();
    public Mock<IOrderRepository> MockOrderRepository { get; } = new();

    public OrderingAppFactory()
    {
        _rabbitMqContainer = TestContainerFactory.CreateRabbitMqContainer();
        _rabbitMqContainer.StartAsync().GetAwaiter().GetResult();

        _postgresContainer = TestContainerFactory.CreatePostgresContainer();
        _postgresContainer.StartAsync().GetAwaiter().GetResult();

        // Make ASPNETCORE_ENVIRONMENT available early (keeps behavior consistent)
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "IntegrationTest");

        TestEnvironmentHelper.SetPostgresConnectionString(_postgresContainer);
        TestEnvironmentHelper.SetRabbitMqHost(_rabbitMqContainer);

        // Force creation of the host now so Services is available to tests immediately.
        // CreateDefaultClient triggers the host to build and run (in-memory test server).
        // We don't keep the HttpClient here — we just ensure host creation now.
        // (This will call ConfigureWebHost below.)
        _ = CreateDefaultClient();
        
        DatabaseHelper.ApplyMigrations<OrderingDbContext>(Services);
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