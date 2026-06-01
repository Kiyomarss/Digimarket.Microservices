using Basket_Application.RepositoryContracts;
using Basketing.Domain.Entities;
using BuildingBlocks.Caching;

namespace Basketing.Infrastructure.Repositories;

public class CachedBasketRepository : IBasketRepository
{
    private readonly IBasketRepository _basketRepository;
    private readonly ICacheService _cache;

    public CachedBasketRepository(
        IBasketRepository basketRepository,
        ICacheService cache)
    {
        _basketRepository = basketRepository;
        _cache = cache;
    }

    public Task<BasketEntity?> FindBasketByUserId(Guid userId)
    {
        return _cache.GetOrSetAsync(
                                    key: CacheKey.Basket(userId),
                                    factory: () => _basketRepository.FindBasketByUserId(userId),
                                    ttl: TimeSpan.FromMinutes(15),
                                    tags: new[] { "basket" }
                                   );
    }

    public Task<BasketItem?> FindBasketItemById(Guid id)
    {
        return _cache.GetOrSetAsync(
                                    key: CacheKey.BasketItem(id),
                                    factory: () => _basketRepository.FindBasketItemById(id),
                                    ttl: TimeSpan.FromMinutes(30),
                                    tags: new[] { "basket-item" }
                                   );
    }

    public async Task<bool> DeleteBasketItemByUserId(Guid userId)
    {
        var result = await _basketRepository.DeleteBasketItemByUserId(userId);

        if (result)
        {
            await _cache.RemoveAsync(CacheKey.Basket(userId));
        }

        return result;
    }

    public async Task<BasketEntity> AddItemToBasket(BasketItem item)
    {
        await _basketRepository.AddItemToBasket(item);

        // حذف کش قبلی
        await _cache.RemoveAsync(CacheKey.Basket(item.Basket.UserId));

        // بازیابی نسخه جدید برای بازگرداندن به کلاینت و قرار دادن در کش
        var updatedBasket = await _basketRepository.FindBasketByUserId(item.Basket.UserId);

        await _cache.SetAsync(CacheKey.Basket(item.Basket.UserId), updatedBasket, TimeSpan.FromMinutes(15));

        return updatedBasket;
    }
}