using Basket.Core.Domain.Entities;

namespace Basket.Core.Domain.RepositoryContracts;

public interface IBasketRepository
{
    Task<BasketEntity> FindBasketByUserId(Guid userId);

    Task AddItemToBasket(BasketItem item);
    
    Task<BasketItem?> FindBasketItemById(Guid id);

    Task<bool> DeleteBasketItem(Guid id);
}