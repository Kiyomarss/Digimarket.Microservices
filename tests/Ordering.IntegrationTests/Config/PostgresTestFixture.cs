using Microsoft.EntityFrameworkCore;
using Ordering_Infrastructure.Data.DbContext;
using Testcontainers.PostgreSql;

namespace Ordering.IntegrationTests.Config;

public class PostgresTestFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; private set; }
    private DbContextOptions<OrderingDbContext> _options;

    public DbContextOptions<OrderingDbContext> Options => _options;

    public async Task InitializeAsync()
    {
        // ساخت کانتینر PostgreSQL
        Container = new PostgreSqlBuilder()
                    .WithUsername("postgres")
                    .WithPassword("postgres")
                    .WithDatabase("ordering_test")
                    .Build();

        await Container.StartAsync();

        // تنظیم DbContext برای اتصال به PostgreSQL واقعی
        _options = new DbContextOptionsBuilder<OrderingDbContext>()
                   .UseNpgsql(Container.GetConnectionString())
                   .Options;

        // اجرای Migration ها
        using var db = new OrderingDbContext(_options);
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }

    // ساخت DbContext جدید برای هر تست
    public OrderingDbContext CreateDbContext()
        => new OrderingDbContext(_options);
}