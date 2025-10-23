using Ordering_Infrastructure.Data.DbContext;
using Ordering.Components.Domain.Entities;
using Ordering.Components.Domain.RepositoryContracts;

namespace Ordering_Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    readonly OrderingDbContext _db;

    public OrderRepository(OrderingDbContext dbContext)
    {
        _db = dbContext;
    }
    
    public async Task AddOrder(Ordering.Components.Domain.Entities.Order order)
    {
        await _db.Set<Ordering.Components.Domain.Entities.Order>().AddAsync(order);
        await _db.SaveChangesAsync();
    }
}