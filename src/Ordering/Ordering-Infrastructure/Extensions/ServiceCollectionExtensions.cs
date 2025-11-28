using BuildingBlocks.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ordering_Domain.Domain.RepositoryContracts;
using Ordering_Infrastructure.Data.DbContext;
using Ordering_Infrastructure.Repositories;

namespace Ordering_Infrastructure.Extensions
{
    public static class OrderingInfrastructureExtensions
    {
        /// <summary>
        /// Registers Ordering infrastructure.
        /// In IntegrationTest environment it avoids registering the production Postgres DbContext
        /// and registers an in-memory DbContext so tests can resolve repository services.
        /// Call: services.AddOrderingInfrastructure(Configuration, env);
        /// </summary>
        public static IServiceCollection AddOrderingInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            var connectionString = configuration.GetConnectionString("Default");

            if (env.IsEnvironment("IntegrationTest"))
            {
                services.AddDbContext<OrderingDbContext>(options =>
                {
                    options.UseInMemoryDatabase("OrderingDb_InMemory");
                });
            }
            else
            {
                services.AddDbContext<OrderingDbContext>(options =>
                {
                    options.UseNpgsql(connectionString, opts =>
                    {
                        opts.MigrationsAssembly(typeof(OrderingDbContext).Assembly.GetName().Name);
                        opts.MigrationsHistoryTable($"__{nameof(OrderingDbContext)}");
                        opts.EnableRetryOnFailure(5);
                        opts.MinBatchSize(1);
                    });
                });
            }

            // Register repository & unit of work (they depend on OrderingDbContext)
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork<OrderingDbContext>>();
            return services;
        }
    }
}
