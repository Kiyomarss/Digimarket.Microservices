using Basket.Components.Contracts;

namespace Basket.Components;

public interface IShoppingCartService
{
    Task<ShoppingCart> SubmitBaskets();
}