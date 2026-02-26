using Catalog_Domain.Entities;
using Catalog_Infrastructure.Data.DbContext;
using Catalog.Application.Products.Queries;
using Catalog.Application.RepositoryContracts;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    readonly CatalogDbContext _db;

    public ProductRepository(CatalogDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<List<Product>> GetProductByIds(IEnumerable<Guid> productIds, CancellationToken ct)
    {
        return await _db.Set<Product>()
                        .AsNoTracking()
                        .Where(x => productIds.Contains(x.Id))
                        .ToListAsync(ct);
    }

    public async Task<IEnumerable<ProductDto>> GetProductByIds2(IEnumerable<Guid> productIds,
                                                               CancellationToken ct)
    {
        return await _db.Set<Product>()
                        .AsNoTracking()
                        .Where(x => productIds.Contains(x.Id))
                        .Select(x => new ProductDto(x.Id, x.Price))
                        .ToListAsync(ct);
    }
    
    public async Task<Product?> FindProductById(Guid id)
    {
        return await _db.Set<Product>().FindAsync(id);
    }
    
    public async Task AddProduct(Product product)
    {
        await _db.Set<Product>().AddAsync(product);
    }
}