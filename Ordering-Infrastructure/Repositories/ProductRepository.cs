using Ordering_Infrastructure.Data.DbContext;
using Ordering.Components.Domain.Entities;

namespace Catalog.Components.Repositories;

public class OrderRepository : IOrderRepository
{
    readonly OrderingDbContext _db;

    public OrderRepository(OrderingDbContext dbContext)
    {
        _db = dbContext;
    }
    
    public async Task AddOrder(Order order)
    {
        await _db.Set<Order>().AddAsync(order);
        await _db.SaveChangesAsync();
    }
}