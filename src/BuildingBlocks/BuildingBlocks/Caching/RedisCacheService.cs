using BuildingBlocks.Extensions.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace BuildingBlocks.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var bytes = await _cache.GetAsync(key);
        return CacheSerialization.FromBytes<T>(bytes);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? ttl = null,
        params string[] tags)
    {
        var bytes = CacheSerialization.ToBytes(value);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow =
                ttl ?? TimeSpan.FromMinutes(10)
        };

        await _cache.SetAsync(key, bytes, options);

        foreach (var tag in tags)
        {
            await AddTag(tag, key);
        }
    }

    public Task RemoveAsync(string key)
        => _cache.RemoveAsync(key);

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? ttl = null,
        params string[] tags)
    {
        var cached = await GetAsync<T>(key);

        if (cached != null)
            return cached;

        var result = await factory();

        if (result != null)
            await SetAsync(key, result, ttl, tags);

        return result;
    }

    private async Task AddTag(string tag, string key)
    {
        var tagKey = CacheKey.Tag(tag);

        var existing =
            await GetAsync<HashSet<string>>(tagKey)
            ?? new HashSet<string>();

        existing.Add(key);

        await SetAsync(tagKey, existing, TimeSpan.FromHours(1));
    }

    public async Task RemoveByTagAsync(string tag)
    {
        var tagKey = CacheKey.Tag(tag);

        var keys = await GetAsync<HashSet<string>>(tagKey);

        if (keys == null)
            return;

        foreach (var key in keys)
            await RemoveAsync(key);

        await RemoveAsync(tagKey);
    }
}