using Basket.Domain.RepositoryContracts;
using BuildingBlocks.CQRS;
using BuildingBlocks.Services; // ICurrentUserService
using OrderGrpc;

namespace Basket_Application.Orders.Commands.CreateOrder;

public class CreateOrderHandler(IBasketRepository basketRepository,
                                OrderProtoService.OrderProtoServiceClient orderProto,
                                ICurrentUserService currentUser) 
    : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();
        if (userId == null)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var basket = await basketRepository.FindBasketByUserId(userId.Value);
        if (basket == null) throw new Exception("Basket not found.");

        var request = new CreateOrderRequest
        {
            Customer = currentUser.GetUserName() ?? "unknown"
        };

        request.Items.AddRange(basket.Items.Select(x => new OrderItemDto
        {
            ProductId = x.ProductId.ToString(),
            Quantity = x.Quantity
        }));

        var response = await orderProto.CreateOrderAsync(request, cancellationToken: cancellationToken);

        return new CreateOrderResult(response.OrderId);
    }
}