using System.Reflection;
using BuildingBlocks.Extensions;
using BuildingBlocks.UnitOfWork;
using Catalog_Domain.RepositoryContracts;
using Catalog_Infrastructure.Data.DbContext;
using Catalog_Infrastructure.Repositories;
using Catalog.Application.Products.Queries;
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
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                options.MigrationsHistoryTable($"__{nameof(CatalogDbContext)}");

                options.EnableRetryOnFailure(5);
                options.MinBatchSize(1);
            });
        });

        // MassTransit + Outbox
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<CatalogDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((_, cfg) =>
            {
                cfg.AutoStart = true;
            });
        });
        
        services.AddConfiguredMediatR(typeof(GetProductsByIdsHandler));

        // Scoped Services
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork<CatalogDbContext>>();
        return services;
    }
}