// tests/Ordering.Application.IntegrationTests/Fixtures/OrderingIntegrationFixture.cs
using DotNet.Testcontainers.Builders;
using MassTransit;
using Testcontainers.PostgreSql; // <--- این using ضروری است
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Ordering_Infrastructure.Data.DbContext;
using Ordering_Infrastructure.Extensions;
using Ordering.Core.Orders.Commands.CreateOrder;
using Respawn;

namespace Ordering.Application.IntegrationTests.Fixtures;

public class OrderingIntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer; // <--- نوع درست
    private Respawner? _respawner;
    public IServiceProvider Services { get; private set; } = default!;
    public ITestHarness TestHarness => Services.GetRequiredService<ITestHarness>();

    public string ConnectionString { get; }

    public OrderingIntegrationFixture()
    {
        // استفاده از PostgreSqlBuilder مخصوص دیتابیس
        _postgresContainer = new PostgreSqlBuilder()
                             .WithImage("postgres:16")
                             .WithDatabase("OrderingDb")
                             .WithUsername("postgres")
                             .WithPassword("123")
                             .WithPortBinding(5432, true)
                             .WithWaitStrategy(Wait.ForUnixContainer()
                                                   .UntilDatabaseIsAvailable(NpgsqlFactory.Instance))
                             .Build();

        _postgresContainer.StartAsync().GetAwaiter().GetResult();

        var mappedPort = _postgresContainer.GetConnectionString(); // این متد خودش ConnectionString کامل رو می‌ده!
        ConnectionString = mappedPort;
    }

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        services.AddDbContext<OrderingDbContext>(options =>
            options.UseNpgsql(ConnectionString));

        services.AddOrderingInfrastructure(new ConfigurationBuilder().Build());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));
        services.AddMassTransitTestHarness();

        Services = services.BuildServiceProvider();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        await dbContext.Database.MigrateAsync();

        _respawner = await Respawner.CreateAsync(ConnectionString);
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is not null)
            await _respawner.ResetAsync(ConnectionString);
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}