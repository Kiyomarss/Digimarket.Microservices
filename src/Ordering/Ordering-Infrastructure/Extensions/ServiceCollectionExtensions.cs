using BuildingBlocks.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering_Domain.Domain.RepositoryContracts;
using Ordering_Infrastructure.Data.DbContext;
using Ordering_Infrastructure.Data.Persistence;
using Ordering_Infrastructure.Repositories;

namespace Ordering_Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        services.AddDbContext<OrderingDbContext>(x =>
        {
            x.UseNpgsql(connectionString, options =>
            {
                options.MigrationsAssembly(typeof(OrderingDbContext).Assembly.GetName().Name);
                options.MigrationsHistoryTable($"__{nameof(OrderingDbContext)}");

                options.EnableRetryOnFailure(5);
                options.MinBatchSize(1);
            });
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}