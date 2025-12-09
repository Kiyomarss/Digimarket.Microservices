using System.Reflection;
using BuildingBlocks.Common.Interceptors;
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
        
        services.AddSingleton<DomainEventsInterceptor>();
        
        services.AddDbContext<OrderingDbContext>(x =>
        {
            var connectionString = configuration.GetConnectionString("Default");

            x.UseNpgsql(connectionString, options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                options.MigrationsHistoryTable($"__{nameof(OrderingDbContext)}");

                options.EnableRetryOnFailure(5);
                options.MinBatchSize(1);
            });
            x.AddInterceptors(services.BuildServiceProvider().GetRequiredService<DomainEventsInterceptor>());
        });

        // MediatR و Pipeline Behaviors
        services.AddConfiguredMediatR(typeof(CreateOrderCommandHandler));
        
        //TODO: در پروژه زیر از دستور زیر استفاده نشده و کد به درستی کار می‌کرد. این مشکل با توجه به نیازی که در تست Worker بود به صورت زیر نوشته شد. در صورت اصلاح می‌توان بخش زیر و ورودی بخش بالا را اصلاح نمود
        //D:\Repository\aspnetcore-microservices
        services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>();

        // gRPC Client برای Product
        services.AddGrpcClientWithConfig<ProductProtoService.ProductProtoServiceClient>(
                                                                                        configuration, "GrpcSettings:CatalogUrl");
        services.AddScoped<IProductService, ProductGrpcService>();
        
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork<OrderingDbContext>>();
        
        return services;
    }
}