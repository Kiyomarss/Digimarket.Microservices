using Ordering_Domain.Domain.Entities;

namespace Ordering_Domain.Domain.RepositoryContracts;

public interface IOrderRepository
{
    Task AddOrder(Order order);

    Task<Order?> FindOrderById(Guid id);

    Task<List<Order>> GetOrdersForUserAsync(Guid userId, string state, CancellationToken cancellationToken);
}