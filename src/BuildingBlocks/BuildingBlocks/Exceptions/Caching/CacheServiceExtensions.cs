using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Caching;
using BuildingBlocks.Configurations;

namespace BuildingBlocks.Extensions;

public static class CacheServiceExtensions
{
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));

        return services;
    }
}