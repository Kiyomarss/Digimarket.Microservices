using Grpc.Core;
using MediatR;
using OrderGrpc;
using Ordering.Application.Orders.Commands.CreateOrder;

namespace Ordering.Api.Grpc;

public class OrderGrpcService : OrderProtoService.OrderProtoServiceBase
{
    private readonly ISender _sender;

    public OrderGrpcService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        var command = new CreateOrderCommand
        {
            Items = request.Items
                           .Select(i => new CreateOrderCommand.OrderItemDto
                           {
                               ProductId = i.ProductId,
                               Quantity = i.Quantity
                           }).ToList()
        };

        var orderId = await _sender.Send(command);

        return new CreateOrderResponse
        {
            OrderId = orderId.ToString()
        };
    }
}