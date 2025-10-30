using BuildingBlocks.CQRS;

namespace Basket_Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand() : ICommand<CreateOrderResult>;

public record CreateOrderResult(string Id);