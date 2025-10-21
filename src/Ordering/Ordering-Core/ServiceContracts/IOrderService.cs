using Ordering.Components.Domain.Entities;
using Ordering.Components.DTO;

namespace Ordering.Components.ServiceContracts;

public interface IOrderService
{
    Task<Order> SubmitOrders(List<OrderItemDto> orderItemDtos);
}