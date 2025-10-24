using Catalog_Infrastructure.Data.DbContext;
using Catalog.Core.Domain.Entities;
using Catalog.Core.Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    readonly CatalogDbContext _db;

    public ProductRepository(CatalogDbContext dbContext)
    {
        _db = dbContext;
    }
    
    public async Task<List<Product>> GetProductByIds(List<Guid> productIds)
    {
        return await _db.Set<Product>()
                        .Where(x => productIds.Contains(x.Id))
                        .ToListAsync();;
    }
    
    public async Task<Product?> FindProductById(Guid id)
    {
        return await _db.Set<Product>().FindAsync(id);
    }
    
    public async Task<bool> DeleteProduct(Guid id)
    {
        var rowsDeleted = await _db.Set<Product>()
                                   .Where(b => b.Id == id)
                                   .ExecuteDeleteAsync();

        return rowsDeleted > 0;
    }
    
    public async Task AddProduct(Product product)
    {
        await _db.Set<Product>().AddAsync(product);
        await _db.SaveChangesAsync();
    }
}