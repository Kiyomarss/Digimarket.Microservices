// tests/Ordering.Api.IntegrationTests/Fixtures/OrderingApiFactory.cs

using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Ordering.Application.Orders.Consumers;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.DbContext;
using Ordering.Worker.Extensions;
using Ordering.Worker.StateMachines;
using Respawn;
using Shared.TestFixtures;

namespace Ordering.Worker.PersistenceTests.Fixtures;

public class WorkerAppFactory : IAsyncLifetime
{
    private IHost _host = default!;
    private readonly IContainer _postgresContainer;

    private Respawner _respawner = default!;
    private string _connectionString = default!;
    private string _dbName = default!;

    public IServiceProvider Services => _host.Services;

    public WorkerAppFactory()
    {
        _postgresContainer = TestContainerFactory.CreatePostgresContainer();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        // ---- Create random DB ----
        _dbName = $"OrderingDb_{Guid.NewGuid():N}";
        var adminConn = $"Host=localhost;Port={_postgresContainer.GetMappedPublicPort(5432)};" +
                        $"Database=postgres;Username=postgres;Password=123;";

        await using (var admin = new NpgsqlConnection(adminConn))
        {
            await admin.OpenAsync();
            await new NpgsqlCommand($"CREATE DATABASE \"{_dbName}\";", admin).ExecuteNonQueryAsync();
        }
        
        // ---- THIS replaces WebApplicationFactory<Program> ----
        var builder = Host.CreateApplicationBuilder();
        
        // ---- Build connection string ----
        _connectionString =
            $"Host=localhost;Port={_postgresContainer.GetMappedPublicPort(5432)};" +
            $"Database={_dbName};Username=postgres;Password=123;";

        Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", _connectionString);

        // register all worker services exactly like Program.cs:
        builder.Services.AddOrderingServices(builder.Configuration);

        // ---- override services for TEST ----
        OverrideServicesForTesting(builder.Services);

        // ---- build the real host ----
        _host = builder.Build();
        await _host.StartAsync();

        // run migrations
        using var scope = _host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersSagaDbContext>();
        await db.Database.MigrateAsync();

        // prepare Respawner
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    private void OverrideServicesForTesting(IServiceCollection services)
    {
        // Remove existing DbContext
        services.RemoveAll<DbContextOptions<OrdersSagaDbContext>>();

        services.AddDbContext<OrdersSagaDbContext>(options =>
        {
            options.UseNpgsql(_connectionString, sql =>
            {
                sql.MigrationsAssembly("Ordering.Worker");
            });
        });

        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
               .EntityFrameworkRepository(r =>
               {
                   r.ExistingDbContext<OrdersSagaDbContext>();
               });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();

        await _postgresContainer.StopAsync();
        await _postgresContainer.DisposeAsync();
    }
}