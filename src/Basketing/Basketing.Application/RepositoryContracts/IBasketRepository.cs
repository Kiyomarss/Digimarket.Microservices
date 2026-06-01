using Basketing.Domain.Entities;

namespace Basket_Application.RepositoryContracts;

public interface IBasketRepository
{
    Task<BasketEntity> FindBasketByUserId(Guid userId);
    
    Task<BasketEntity> AddItemToBasket(BasketItem item);
    
    Task<BasketItem?> FindBasketItemById(Guid id);

    Task<bool> DeleteBasketItemByUserId(Guid userId);
}