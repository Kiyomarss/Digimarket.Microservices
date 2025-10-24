namespace Ordering.Components.Domain.RepositoryContracts;

public interface IOrderRepository
{
    Task AddOrder(Entities.Order order);
}