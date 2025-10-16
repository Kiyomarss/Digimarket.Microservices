namespace Basket.Components;

public interface IBasketUpdaterService
{
    Task AddItem(Guid catalogId, int quantity);

    Task RemoveItem(Guid id);
}