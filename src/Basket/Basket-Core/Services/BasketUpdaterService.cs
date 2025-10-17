using Basket.Core.Domain.RepositoryContracts;
using Basket.Core.ServiceContracts;

namespace Basket.Core;

public class BasketUpdaterService : IBasketUpdaterService
{
    readonly IBasketRepository _basketRepository;

    public BasketUpdaterService(IBasketRepository basketRepository)
    {
        _basketRepository = basketRepository;
    }

    public async Task AddItem(Guid catalogId, int quantity)
    {
        var basket = await _basketRepository.FindBasketByUserId(new Guid("d56be165-fe7d-460c-ad0a-016d90a31dbc")); // userId واقعی را از claims بگیر
        if (basket == null) throw new Exception("Basket not found.");

        var item = new BasketItem
        {
            CatalogId = catalogId,
            Quantity = quantity,
            BasketId = basket.Id
        };

        await _basketRepository.AddItemToBasket(item);
    }
    
    public async Task RemoveItem(Guid id)
    {
        var entity = await _basketRepository.FindBasketItemById(id);

        if (entity == null)
            throw new Exception("FindAsync not found for user.");

        var deleted = await _basketRepository.DeleteBasketItem(id);
        
        if (!deleted)
            throw new Exception($"No BasketItem found with Id = {id}");
    }
}