using BuildingBlocks.Extensions;
using BuildingBlocks.UnitOfWork;
using Identity.Application.RepositoryContracts;
using Identity.Infrastructure.Data.DbContext;
using Identity.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();

        services.AddDbContext<IdentityDbContext>(options =>
        {
            var connectionString =
                configuration.GetConnectionString("Default");

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("Identity.Infrastructure");

                npgsqlOptions.MigrationsHistoryTable(
                                                     $"__{nameof(IdentityDbContext)}");

                npgsqlOptions.EnableRetryOnFailure(5);

                npgsqlOptions.MinBatchSize(1);
            });

            // ✅ صحیح
            options.UseOpenIddict();
        });

        services.AddConfiguredMediatR();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IUnitOfWork,
            UnitOfWork<IdentityDbContext>>();

        return services;
    }
}