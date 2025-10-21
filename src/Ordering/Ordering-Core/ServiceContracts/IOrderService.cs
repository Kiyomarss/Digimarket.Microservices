using Ordering.Components.Domain.Entities;
using Ordering.Components.DTO;

namespace Ordering.Components.ServiceContracts;

public interface IOrderService
{
    Task<Guid> CreateOrder(OrderDto dto);
}