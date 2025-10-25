using Ordering.Core.Domain.Entities;

namespace Ordering.Core.Domain.RepositoryContracts;

public interface IOrderRepository
{
    Task AddOrder(Order order);
}