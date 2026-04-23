namespace BuildingBlocks.Caching;

public class CacheLock
{
    private static readonly SemaphoreSlim _lock = new(1,1);

    public static async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        await _lock.WaitAsync();

        try
        {
            return await action();
        }
        finally
        {
            _lock.Release();
        }
    }
}