using BuildingBlocks.CQRS;

namespace Basket.Core.Services.CheckoutBasket;

public record CreateOrderCommand() : ICommand<CreateOrderResult>;

public record CreateOrderResult(string Id);