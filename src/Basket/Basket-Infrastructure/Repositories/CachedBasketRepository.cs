using System.Text.Json;
using Basket.Core.Domain.Entities;
using Basket.Core.Domain.RepositoryContracts;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.Infrastructure.Repositories;

/// <summary>
/// Repository with Redis caching layer for Basket operations.
/// </summary>
public class CachedBasketRepository : IBasketRepository
{
    private readonly IBasketRepository _basketRepository;
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _cacheOptions;

    public CachedBasketRepository(IBasketRepository basketRepository, IDistributedCache cache)
    {
        _basketRepository = basketRepository;
        _cache = cache;

        // تنظیمات پیش‌فرض کش (قابل تنظیم از IConfiguration در آینده)
        _cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(3)
        };
    }

    public async Task<BasketEntity> FindBasketByUserId(Guid userId)
    {
        string cacheKey = $"basket:{userId}";

        // ابتدا از کش بخوان
        var cached = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<BasketEntity>(cached)!;

        // اگر در کش نبود، از دیتابیس بخوان
        var basket = await _basketRepository.FindBasketByUserId(userId);

        // در کش ذخیره کن
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(basket), _cacheOptions);

        return basket;
    }

    public async Task<BasketItem?> FindBasketItemById(Guid id)
    {
        string cacheKey = $"basket:item:{id}";

        var cached = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<BasketItem>(cached)!;

        var item = await _basketRepository.FindBasketItemById(id);
        if (item != null)
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(item), _cacheOptions);

        return item;
    }

    public async Task<bool> DeleteBasketItem(Guid id)
    {
        // حذف از دیتابیس
        var result = await _basketRepository.DeleteBasketItem(id);

        if (result)
        {
            // حذف از کش
            await _cache.RemoveAsync($"basket:item:{id}");
        }

        return result;
    }

    public async Task AddItemToBasket(BasketItem item)
    {
        await _basketRepository.AddItemToBasket(item);

        // کش مرتبط با این سبد را حذف کن چون تغییر کرده
        await _cache.RemoveAsync($"basket:{item.BasketId}");
    }
}