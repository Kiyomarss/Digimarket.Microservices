using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Extensions;

public static class WebApplicationMigrationExtensions
{
    public static WebApplication MigrateDatabase<TDbContext>(this WebApplication app)
        where TDbContext : DbContext
    {
        using var scope = app.Services.CreateScope();

        var logger = scope.ServiceProvider
                          .GetRequiredService<ILogger<TDbContext>>();

        var dbContext = scope.ServiceProvider
                             .GetRequiredService<TDbContext>();

        try
        {
            logger.LogInformation(
                                  "Starting database migration for DbContext {DbContextName}",
                                  typeof(TDbContext).Name);

            dbContext.Database.Migrate();

            logger.LogInformation(
                                  "Database migration completed successfully for DbContext {DbContextName}",
                                  typeof(TDbContext).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(
                            ex,
                            "An error occurred while migrating the database for DbContext {DbContextName}",
                            typeof(TDbContext).Name);

            throw;
        }

        return app;
    }
}