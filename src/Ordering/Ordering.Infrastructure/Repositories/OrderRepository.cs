using Microsoft.EntityFrameworkCore;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.Enum;
using Ordering_Domain.Domain.RepositoryContracts;
using Ordering_Infrastructure.Data.DbContext;

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
    }
    
    public async Task<Order?> GetByIdWithItemsAsync(Guid id)
    {
        return await _db.Set<Order>()
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == id);
    }
    
    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _db.Set<Order>()
                        .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetOrdersForUserAsync(Guid userId, OrderState state, CancellationToken ct)
    {
        return await _db.Set<Order>()
                        .Where(x => x.UserId == userId && x.State == state)
                        .Select(x => new Order
                        {
                            Date = x.Date, Items = x.Items
                        })
                        .ToListAsync(ct);
    }
}