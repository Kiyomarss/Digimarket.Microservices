using Microsoft.EntityFrameworkCore;
using Ordering_Infrastructure.Data.DbContext;

namespace Ordering.IntegrationTests.Config;

public class LocalPostgresFixture : IAsyncLifetime
{
    public DbContextOptions<OrderingDbContext> Options { get; private set; }

    public async Task InitializeAsync()
    {
        var connectionString =
            "host=localhost;user id=postgres;password=123;database=ordering_test;";

        Options = new DbContextOptionsBuilder<OrderingDbContext>()
                  .UseNpgsql(connectionString)
                  .Options;

        using var db = new OrderingDbContext(Options);

        // ✅ فقط یک بار مایگریشن
        //await db.Database.MigrateAsync();

        // ✅ پاک‌سازی دیتا قبل از هر ران کامل تست
        //await CleanDatabase(db);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public OrderingDbContext CreateDbContext()
        => new OrderingDbContext(Options);

    // ✅ متد برای پاک‌سازی دیتابیس
    private async Task CleanDatabase(OrderingDbContext db)
    {
        db.OrderItems.RemoveRange(db.OrderItems);
        db.Orders.RemoveRange(db.Orders);

        await db.SaveChangesAsync();
    }
}