using Basket_Application.RepositoryContracts;
using Basket.Domain.Entities;
using BuildingBlocks.Caching;

namespace Basket.Infrastructure.Repositories;

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

    public async Task<bool> DeleteBasketItem(Guid id)
    {
        var result = await _basketRepository.DeleteBasketItem(id);

        if (result)
        {
            await _cache.RemoveAsync(CacheKey.BasketItem(id));
        }

        return result;
    }

    public async Task AddItemToBasket(BasketItem item)
    {
        await _basketRepository.AddItemToBasket(item);

        await _cache.RemoveAsync(CacheKey.Basket(item.BasketId));
    }
}