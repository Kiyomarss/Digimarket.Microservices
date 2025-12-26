using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.Enum;

namespace Ordering_Domain.Domain.RepositoryContracts;

public interface IOrderRepository
{
    Task AddOrder(Order order);

    Task<Order?> GetByIdWithItemsAsync(Guid id);

    Task<Order?> GetByIdAsync(Guid id);

    Task<List<Order>> GetOrdersForUserAsync(Guid userId, OrderState state, CancellationToken cancellationToken);
}