using DotNet.Testcontainers.Containers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.DbContext;
using Ordering.Worker.PersistenceTests.Fixtures;
using Ordering.Worker.StateMachines;
using Respawn;
using Respawn.Graph;
using Shared.TestFixtures;

namespace Ordering.Worker.PersistenceTests.TestBase;

public abstract class WorkerPersistenceTestBase : IAsyncLifetime
{
    protected OrdersSagaDbContext DbContext { get; private set; } = default!;
    protected ISagaStateMachineTestHarness<OrderStateMachine, OrderState> SagaHarness { get; private set; } = default!;
    protected IBusControl Bus { get; private set; } = default!;
    private IContainer _postgresContainer = default!;
    private string _connectionString = default!;
    private Respawner _respawner = default!;

    public async Task InitializeAsync()
    {
        // راه اندازی Postgres container
        _postgresContainer = TestContainerFactory.CreatePostgresContainer();
        await _postgresContainer.StartAsync();

        _connectionString = $"Host=localhost;Port={_postgresContainer.GetMappedPublicPort(5432)};" +
                            $"Database=worker_test;Username=postgres;Password=123;";

        var options = new DbContextOptionsBuilder<OrdersSagaDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        DbContext = new OrdersSagaDbContext(options);
        await DbContext.Database.MigrateAsync();

        // راه اندازی MassTransit InMemoryHarness برای تعامل با Saga
        var harness = new InMemoryTestHarness();
        var machine = new OrderStateMachine();
        var repository = new InMemorySagaRepository<OrderState>();
        SagaHarness = harness.StateMachineSaga(machine, repository);
        Bus = harness.BusControl;

        await harness.Start();

        // Respawner برای reset DB قبل از هر تست
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = new Table[] { new("__EFMigrationsHistory") }
        });
    }

    public async Task ResetDatabaseAsync() => await _respawner.ResetAsync(new NpgsqlConnection(_connectionString));

    public async Task DisposeAsync()
    {
        if (Bus != null) await Bus.StopAsync();
        await _postgresContainer.DisposeAsync();
    }

    protected async Task PublishEventAsync<TEvent>(TEvent @event) where TEvent : class
        => await Bus.Publish(@event);
}