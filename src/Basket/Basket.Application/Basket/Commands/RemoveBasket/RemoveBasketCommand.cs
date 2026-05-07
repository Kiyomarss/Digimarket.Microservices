
using BuildingBlocks.CQRS;

namespace Basket_Application.Basket.Commands.RemoveBasket;

public class RemoveBasketCommand : ICommand
{
    public Guid UserId { get; set; }
}