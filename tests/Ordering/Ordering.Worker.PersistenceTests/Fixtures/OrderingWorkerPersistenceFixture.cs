using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines;
using DotNet.Testcontainers.Containers;
using MassTransit.EntityFrameworkCoreIntegration;
using Ordering.Worker.DbContext;
using Respawn;
using Respawn.Graph;
using Shared.TestFixtures;

namespace Ordering.Worker.PersistenceTests.Fixtures;

public class OrderingWorkerPersistenceFixture : IAsyncDisposable, IAsyncLifetime
{
    private readonly IContainer _postgresContainer;
    private string _dbName = default!;
    private string _connectionString = default!;
    private Respawner _respawner = default!;

    public IBusControl Bus { get; private set; } = default!;
    public ISagaStateMachineTestHarness<OrderStateMachine, OrderState> SagaHarness { get; private set; } = default!;
    public OrdersSagaDbContext DbContext { get; private set; } = default!;

    public OrderingWorkerPersistenceFixture()
    {
        // ایجاد container برای Postgres
        _postgresContainer = TestContainerFactory.CreatePostgresContainer();
        _postgresContainer.StartAsync().GetAwaiter().GetResult();
    }

    public async Task InitializeAsync()
    {
        _dbName = $"OrderingDb_{Guid.NewGuid():N}";

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

        _connectionString =
            $"Host=localhost;Port={_postgresContainer.GetMappedPublicPort(5432)};" +
            $"Database={_dbName};Username=postgres;Password=123;";

        // DbContext واقعی برای Worker (SagaDbContext)
        var options = new DbContextOptionsBuilder<OrdersSagaDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        DbContext = new OrdersSagaDbContext(options);
        await DbContext.Database.MigrateAsync();

        // راه‌اندازی MassTransit InMemory TestHarness
        var harness = new InMemoryTestHarness { TestTimeout = TimeSpan.FromSeconds(30) };
        harness.OnConfigureInMemoryBus += cfg => cfg.UseDelayedMessageScheduler();

        var machine = new OrderStateMachine();
        var repository = new InMemorySagaRepository<OrderState>();
        SagaHarness = harness.StateMachineSaga(machine, repository);

        await harness.Start();

        Bus = harness.BusControl; // Bus بعد از Start مقداردهی شود
        
        // Respawner برای ریست دیتابیس قبل از هر تست
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = new []
            {
                new Table("__EFMigrationsHistory")
            }
        });
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public async Task ResetDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
    }

    public async ValueTask DisposeAsync()
    {
        if (Bus != null)
            await Bus.StopAsync();

        await _postgresContainer.DisposeAsync();
    }
    
    protected async Task PublishEventAsync<TEvent>(TEvent @event) where TEvent : class
        => await Bus.Publish(@event);
}