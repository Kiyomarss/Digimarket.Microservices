namespace BuildingBlocks.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? ttl = null,
        params string[] tags);

    Task RemoveAsync(string key);

    Task RemoveByTagAsync(string tag);

    Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? ttl = null,
        params string[] tags);
}