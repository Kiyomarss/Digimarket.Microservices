using Microsoft.EntityFrameworkCore;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Core.Domain.Entities;
using Ordering.Core.Domain.RepositoryContracts;

namespace Ordering_Infrastructure.Repositories;

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
    
    public async Task<Order?> FindOrderById(Guid id)
    {
        return await _db.Set<Order>()
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == id);
    }
    
    public async Task Update()
    {
        await _db.SaveChangesAsync();
    }
}