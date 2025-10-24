using Ordering_Infrastructure.Data.DbContext;
using Ordering.Core.Domain.RepositoryContracts;

namespace Ordering_Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    readonly OrderingDbContext _db;

    public OrderRepository(OrderingDbContext dbContext)
    {
        _db = dbContext;
    }
    
    public async Task AddOrder(Ordering.Core.Domain.Entities.Order order)
    {
        await _db.Set<Ordering.Core.Domain.Entities.Order>().AddAsync(order);
        await _db.SaveChangesAsync();
    }
}