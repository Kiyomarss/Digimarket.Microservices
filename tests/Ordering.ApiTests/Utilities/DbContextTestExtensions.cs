using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ordering_Infrastructure.Data.DbContext;

namespace Ordering.ApiTests.Utilities;

public static class DbContextTestExtensions
{
    public static void ReplaceOrderingDbContextWithInMemory(this IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(
                                                  d => d.ServiceType == typeof(DbContextOptions<OrderingDbContext>));

        if (descriptor != null)
            services.Remove(descriptor);

        services.AddDbContext<OrderingDbContext>(options =>
        {
            options.UseInMemoryDatabase("OrderingTestsDb");
        });
    }
}