using BuildingBlocks.Configurations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Extensions.Caching;

public static class CacheExtensions
{
    public static DistributedCacheEntryOptions GetCacheOptions(this IOptions<CacheSettings> settings)
    {
        var s = settings.Value;
        return new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(s.AbsoluteExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(s.SlidingExpirationMinutes)
        };
    }
}