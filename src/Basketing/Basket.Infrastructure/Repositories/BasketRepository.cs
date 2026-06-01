using Basket_Application.RepositoryContracts;
using Basket.Domain.Entities;
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
                                   .Where(i => i.Id == id)
                                   .ExecuteDeleteAsync();

        return rowsDeleted > 0;
    }
    
    public async Task<bool> DeleteBasketItemByUserId(Guid userId)
    {
        var rowsDeleted = await _db.Set<BasketItem>()
                                   .Where(i => i.Basket.UserId == userId)
                                   .ExecuteDeleteAsync();

        return rowsDeleted > 0;
    }

    
    public async Task<BasketEntity> AddItemToBasket(BasketItem item)
    {
        await _db.Set<BasketItem>().AddAsync(item);

        return item.Basket;
    }
}