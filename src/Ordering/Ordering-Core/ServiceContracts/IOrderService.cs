using Ordering.Core.DTO;

namespace Ordering.Core.ServiceContracts;

public interface IOrderService
{
    Task<Guid> CreateOrder(OrderDto dto);
}