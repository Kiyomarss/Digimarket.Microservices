// tests/Ordering.Api.IntegrationTests/Fixtures/OrderingApiFactory.cs

using BuildingBlocks.UnitOfWork;
using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Api;
using Ordering.Application.RepositoryContracts;
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
    
    public Mock<IProductService> ProductServiceMock { get; } = new();

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
    }

    // Override ConfigureWebHost to register/override services inside the factory's host
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.IntegrationTest.json", optional: false);
        });
        
        builder.ConfigureServices(services =>
        {
            // ----- Replace real Product gRPC client/service with a mock IProductService -----
            services.RemoveAll<ProductProtoService.ProductProtoServiceClient>();
            services.RemoveAll<IProductService>();
            services.RemoveAll<IProductService>();

            ProductServiceMock
                .Setup(x => x.GetProductsByIdsAsync(
                                                    It.IsAny<IEnumerable<string>>(),
                                                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(DefaultProducts());

            services.AddSingleton(ProductServiceMock.Object);
            
            var currentUserServiceMock = new MockCurrentUserService().WithDefaultUser().Build();
            services.AddSingleton(currentUserServiceMock);
        });
    }
    
    private static GetProductsResponse DefaultProducts()
    {
        var response = new GetProductsResponse();

        response.Products.Add(new ProductInfo
        {
            ProductId = TestGuids.Guid1,
            ProductName = "Test",
            Price = 1500
        });

        response.Products.Add(new ProductInfo
        {
            ProductId = TestGuids.Guid2,
            ProductName = "Another",
            Price = 2500
        });

        return response;
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