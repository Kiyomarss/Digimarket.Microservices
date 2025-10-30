using Basket.Domain.Domain.RepositoryContracts;
using BuildingBlocks.CQRS;
using OrderGrpc;

namespace Basket_Application.Orders.Commands.CreateOrder;
public class CreateOrderHandler(IBasketRepository basketRepository, OrderProtoService.OrderProtoServiceClient orderProto)
    : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.FindBasketByUserId(new Guid("d56be165-fe7d-460c-ad0a-016d90a31dbc"));
        if (basket == null) throw new Exception("Basket not found.");

        var request = new CreateOrderRequest
        {
            Customer = "pasyar"
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