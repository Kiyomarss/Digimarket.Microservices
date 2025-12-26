using System.Reflection;
using BuildingBlocks.Extensions;
using BuildingBlocks.UnitOfWork;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Ordering_Domain.Domain.RepositoryContracts;
using Ordering_Infrastructure.Data.DbContext;
using Ordering_Infrastructure.Repositories;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Application.Services;
using ProductGrpc;

namespace Ordering.Api.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers
        services.AddControllers();
        
        services.AddDbContext<OrderingDbContext>(x =>
        {
            var connectionString = configuration.GetConnectionString("Default");

            x.UseNpgsql(connectionString, options =>
            {
                options.MigrationsAssembly("Ordering.Infrastructure");
                options.MigrationsHistoryTable($"__{nameof(OrderingDbContext)}");

                options.EnableRetryOnFailure(5);
                options.MinBatchSize(1);
            });
        });

        // MediatR و Pipeline Behaviors
        services.AddConfiguredMediatR();

        // gRPC Client برای Product
        services.AddGrpcClientWithConfig<ProductProtoService.ProductProtoServiceClient>(
                                                                                        configuration, "GrpcSettings:CatalogUrl");
        services.AddScoped<IProductService, ProductGrpcService>();
        
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork<OrderingDbContext>>();
        
        return services;
    }
}