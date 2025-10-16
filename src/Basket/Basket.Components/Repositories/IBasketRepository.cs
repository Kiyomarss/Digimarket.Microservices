namespace Basket.Components.Repositories;

public interface IBasketRepository
{
    Task<Basket> FindBasketByUserId(Guid userId);

    Task AddItemToBasket(BasketItem item);
    
    Task<BasketItem?> FindBasketItemById(Guid id);

    Task<bool> DeleteBasketItem(Guid id);
}