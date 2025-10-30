namespace Basket.Domain.ServiceContracts;

public interface IBasketUpdaterService
{
    Task AddItem(Guid catalogId, int quantity);

    Task RemoveItem(Guid id);
}