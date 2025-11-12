using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ordering_Infrastructure.Data.DbContext;

namespace Ordering.ApiTests.Utilities;

public static class DatabaseTestExtensions
{
    public static void ReplaceDbContextWithInMemory(this IServiceCollection services, string dbName)
    {
        services.RemoveAll(typeof(DbContextOptions<OrderingDbContext>));
        services.RemoveAll(typeof(OrderingDbContext));

        // اضافه کردن DbContext اصلی با InMemoryDatabase
        services.AddDbContext<OrderingDbContext>(options =>
        {
            options.UseInMemoryDatabase(dbName);
        });

        // نگاشت OrderingDbContext به TestOrderingDbContext برای کنترل تست
        services.AddScoped<TestOrderingDbContext>(sp =>
        {
            var options = sp.GetRequiredService<DbContextOptions<OrderingDbContext>>();
            return new TestOrderingDbContext(options);
        });
    }
}