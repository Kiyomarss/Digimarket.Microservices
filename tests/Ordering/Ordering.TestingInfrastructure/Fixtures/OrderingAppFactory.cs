// tests/Ordering.Api.IntegrationTests/Fixtures/OrderingApiFactory.cs

using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Npgsql;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Api;
using Ordering.Api.Consumers;
using Ordering.Application.Services;
using ProductGrpc;
using Respawn;
using Shared;
using Shared.TestFixtures;
using Xunit;

namespace Ordering.TestingInfrastructure.Fixtures;

public class OrderingAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IContainer _rabbitMqContainer;
    private readonly IContainer _postgresContainer;
    private Respawner _respawner = default!;
    private string _connectionString = default!;
    private string _dbName = default!;


    // public properties (tests expect to access Services, Bus, DbContext)
    // WebApplicationFactory already exposes "Services", so tests can use `Fixture.Services`
    public IBusControl Bus => Services.GetRequiredService<IBusControl>();
    public OrderingDbContext DbContext => Services.CreateScope().ServiceProvider.GetRequiredService<OrderingDbContext>();
    
    public Mock<IProductService> ProductServiceMock { get; } = new();

    public OrderingAppFactory()
    {
        _rabbitMqContainer = TestContainerFactory.CreateRabbitMqContainer();
        _rabbitMqContainer.StartAsync().GetAwaiter().GetResult();

        _postgresContainer = TestContainerFactory.CreatePostgresContainer();
        _postgresContainer.StartAsync().GetAwaiter().GetResult();

        // Make ASPNETCORE_ENVIRONMENT available early (keeps behavior consistent)
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "IntegrationTest");

        TestEnvironmentHelper.SetRabbitMqHost(_rabbitMqContainer);
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

            ProductServiceMock
                .Setup(x => x.GetProductsByIdsAsync(
                                                    It.IsAny<IEnumerable<string>>(),
                                                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(DefaultProducts());

            services.AddSingleton(ProductServiceMock.Object);
            
            var currentUserServiceMock = new MockCurrentUserService().WithDefaultUser().Build();
            services.AddSingleton(currentUserServiceMock);
            
            // حذف DbContext قبلی
            services.RemoveAll<DbContextOptions<OrderingDbContext>>();

            // اضافه کردن DbContext با connection string رندوم
            services.AddDbContext<OrderingDbContext>(options =>
            {
                options.UseNpgsql(_connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly("Ordering.Infrastructure");
                    sqlOptions.MigrationsHistoryTable($"__{nameof(OrderingDbContext)}");
                });
            });
            
            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumers(typeof(OrderPaidConsumer).Assembly);
            });

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

    public async Task InitializeAsync()
    {
        _dbName = $"OrderingDb_{Guid.NewGuid():N}";

        // connect to postgres system db
        var adminConn =
            $"Host=localhost;Port={_postgresContainer.GetMappedPublicPort(5432)};" +
            $"Database=postgres;Username=postgres;Password=123;";

        await using (var admin = new NpgsqlConnection(adminConn))
        {
            await admin.OpenAsync();

            await new NpgsqlCommand(
                                    $"CREATE DATABASE \"{_dbName}\";",
                                    admin).ExecuteNonQueryAsync();
        }

        // build real connection string
        _connectionString =
            $"Host=localhost;Port={_postgresContainer.GetMappedPublicPort(5432)};" +
            $"Database={_dbName};Username=postgres;Password=123;";

        Environment.SetEnvironmentVariable(
                                           "DATABASE_CONNECTION_STRING",
                                           _connectionString);

        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

        await db.Database.MigrateAsync();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await _respawner.ResetAsync(conn);

        // VACUUM must be executed outside EF transaction
        /*await using var vacuumCmd = new NpgsqlCommand("VACUUM;", conn);
        await vacuumCmd.ExecuteNonQueryAsync();*/
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