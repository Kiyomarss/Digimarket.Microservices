using Basket.Domain.Entities;
using Basket.Domain.RepositoryContracts;
using BuildingBlocks.Configurations;
using BuildingBlocks.Extensions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Basket.Infrastructure.Repositories;
public class CachedBasketRepository : IBasketRepository
{
    private readonly IBasketRepository _basketRepository;
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _cacheOptions;

    public CachedBasketRepository(
        IBasketRepository basketRepository,
        IDistributedCache cache,
        IOptions<CacheSettings> cacheSettings)
    {
        _basketRepository = basketRepository;
        _cache = cache;
        _cacheOptions = cacheSettings.GetCacheOptions();
    }

    public async Task<BasketEntity> FindBasketByUserId(Guid userId)
    {
        string cacheKey = $"basket:{userId}";

        // از کش بخوان
        var cachedBytes = await _cache.GetAsync(cacheKey);
        var cached = CacheSerialization.FromBytes<BasketEntity>(cachedBytes);
        if (cached != null)
            return cached;

        // اگر در کش نبود، از دیتابیس بخوان
        var basket = await _basketRepository.FindBasketByUserId(userId);

        // در کش ذخیره کن
        var bytes = CacheSerialization.ToBytes(basket);
        await _cache.SetAsync(cacheKey, bytes, _cacheOptions);

        return basket;
    }

    public async Task<BasketItem?> FindBasketItemById(Guid id)
    {
        string cacheKey = $"basket:item:{id}";

        var cachedBytes = await _cache.GetAsync(cacheKey);
        var cached = CacheSerialization.FromBytes<BasketItem>(cachedBytes);
        if (cached != null)
            return cached;

        var item = await _basketRepository.FindBasketItemById(id);
        if (item != null)
        {
            var bytes = CacheSerialization.ToBytes(item);
            await _cache.SetAsync(cacheKey, bytes, _cacheOptions);
        }

        return item;
    }

    public async Task<bool> DeleteBasketItem(Guid id)
    {
        var result = await _basketRepository.DeleteBasketItem(id);
        if (result)
            await _cache.RemoveAsync($"basket:item:{id}");
        return result;
    }

    public async Task AddItemToBasket(BasketItem item)
    {
        await _basketRepository.AddItemToBasket(item);
        await _cache.RemoveAsync($"basket:{item.BasketId}");
    }
}