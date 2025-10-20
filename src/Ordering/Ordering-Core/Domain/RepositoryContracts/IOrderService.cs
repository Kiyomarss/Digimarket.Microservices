using Ordering.Components.Domain.Entities;
using Ordering.Components.DTO;

namespace Ordering.Components.Domain.RepositoryContracts;

public interface IOrderService
{
    Task<Order> SubmitOrders(List<OrderItemDto> orderItemDtos);
    Task SubmitOrders2(Guid orderId);
}