using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.Enum;
using Ordering.Application.Orders.Queries;

namespace Ordering.Application.RepositoryContracts;

public interface IOrderRepository
{
    Task Add(Order order);

    Task<Order?> GetByIdWithItemsAsync(Guid id);

    Task<Order?> GetByIdAsync(Guid id);

    Task<List<OrderSummaryDto>> GetOrdersForUserAsync(Guid userId, OrderState state, CancellationToken cancellationToken);
}