using Ordering.Components.Domain.Entities;

namespace Catalog.Components.Repositories;

public interface IOrderRepository
{
    Task AddOrder(Order order);
}