using BuildingBlocks.Extensions;
using BuildingBlocks.UnitOfWork;
using Catalog_Infrastructure.Data.DbContext;
using Catalog_Infrastructure.Repositories;
using Catalog.Application.RepositoryContracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers
        services.AddControllers();

        // Database Context
        services.AddDbContext<CatalogDbContext>(x =>
        {
            var connectionString = configuration.GetConnectionString("Default");

            x.UseNpgsql(connectionString, options =>
            {
                options.MigrationsAssembly(typeof(CatalogDbContext).Assembly.GetName().Name);
                options.MigrationsHistoryTable($"__{nameof(CatalogDbContext)}");

                options.EnableRetryOnFailure(5);
                options.MinBatchSize(1);
            });
        });
        
        services.AddConfiguredMediatR();

        // Scoped Services
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork<CatalogDbContext>>();

        return services;
    }
}