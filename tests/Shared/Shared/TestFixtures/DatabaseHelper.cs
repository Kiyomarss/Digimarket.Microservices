using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Shared.TestFixtures
{
    public static class DatabaseHelper
    {
        public static void ApplyMigrations<TContext>(IServiceProvider services) 
            where TContext : DbContext
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TContext>();
            db.Database.Migrate();
        }
    }
}