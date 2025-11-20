// tests/Ordering.Api.IntegrationTests/Fixtures/OrderingApiFactory.cs
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ordering_Infrastructure.Data.DbContext;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ordering.Api.IntegrationTests.Fixtures;

public class OrderingApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("OrderingDb")
        .WithUsername("postgres")
        .WithPassword("123")
        .WithPortBinding(5432, true) // پورت رندم روی هاست — بدون تداخل با لوکال
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready -U postgres"))
        .Build();

    private Respawner? _respawner;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest"); // محیط تست

        builder.ConfigureServices(services =>
        {
            // جایگزینی DbContext با کانتینر واقعی
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<OrderingDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<OrderingDbContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString()));

            // اگر می‌خوای gRPC Client خارجی (Catalog) رو Mock کنی، اینجا انجام بده
            // services.Remove(services.SingleOrDefault(s => s.ServiceType == typeof(IProductService)));
            // services.AddScoped<IProductService, MockProductService>();
        });
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        // اعمال Migrationها
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        await dbContext.Database.MigrateAsync();

        // آماده‌سازی Respawn برای Reset دیتابیس بین تست‌ها
        _respawner = await Respawner.CreateAsync(_postgresContainer.GetConnectionString());
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is not null)
            await _respawner.ResetAsync(_postgresContainer.GetConnectionString());
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}