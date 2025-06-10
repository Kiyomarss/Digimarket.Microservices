using Ordering.Components.Contracts;

namespace Ordering.Components;

public interface IOrderService
{
    Task<Order> SubmitOrders(List<OrderItemDto> orderItemDtos);
    Task SubmitOrders2(Guid orderId);
}