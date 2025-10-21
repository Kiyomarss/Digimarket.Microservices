using Basket.Core.Domain.Entities;
using Basket.Core.Domain.RepositoryContracts;
using Basket.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Basket.Infrastructure.Repositories;

public class BasketRepository : IBasketRepository
{
    readonly BasketDbContext _db;

    public BasketRepository(BasketDbContext dbContext)
    {
        _db = dbContext;
    }
    
    public async Task<BasketEntity> FindBasketByUserId(Guid userId)
    {
        return await _db.Set<BasketEntity>()
                        .Include(b => b.Items)
                        .SingleAsync(x => x.UserId == userId);
    }
    
    public async Task<BasketItem?> FindBasketItemById(Guid id)
    {
        return await _db.Set<BasketItem>().FindAsync(id);
    }
    
    public async Task<bool> DeleteBasketItem(Guid id)
    {
        var rowsDeleted = await _db.Set<BasketItem>()
                                   .Where(b => b.Id == id)
                                   .ExecuteDeleteAsync();

        return rowsDeleted > 0;
    }
    
    public async Task AddItemToBasket(BasketItem item)
    {
        await _db.Set<BasketItem>().AddAsync(item);
        await _db.SaveChangesAsync();
    }
}