using Grpc.Core;
using Order.Grpc;

namespace Ordering.Components.Services;

public class OrderService2
    ()
    : OrderProtoService.OrderProtoServiceBase
{    
    public override Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {

        return Task.FromResult(new CreateOrderResponse(){OrderId = "sdgs"});
    }
}
