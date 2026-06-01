using Basketing.Domain.Entities;

namespace Basket_Application.RepositoryContracts;

public interface IBasketRepository
{
    Task<Basketing.Domain.Entities.Basket> FindBasketByUserId(Guid userId);
    
    Task<Basketing.Domain.Entities.Basket> AddItemToBasket(BasketItem item);
    
    Task<BasketItem?> FindBasketItemById(Guid id);

    Task<bool> DeleteBasketItemByUserId(Guid userId);
}