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
    
    //TODO: یک کلاس بیس تعریف شود و متد های پایه ای مانند زیر به آن انتقال داده شود
    public async Task AddOrder(Order order)
    {
        await _db.Set<Order>().AddAsync(order);
    }
    
    //TODO: در تمام کد هایی که برای ریپوزیتوری نوشته شده از Set استفاده کرده ام. می‌تواز دستورات دیگری مانند Find، Add و ... استفاده کرد. در را بطه با این مورد تحقیق شود.
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