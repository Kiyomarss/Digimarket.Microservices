using Catalog_Domain.Entities;
using Catalog.Application.Products.Queries;

namespace Catalog.Application.RepositoryContracts;

public interface IProductRepository
{
    public Task<List<Product>> GetProductByIds(IEnumerable<Guid> productIds, CancellationToken ct);

    public Task<IEnumerable<ProductDto>> GetProductByIds2(IEnumerable<Guid> productIds, CancellationToken ct);
    
    Task AddProduct(Product product);
    
    Task<Product?> FindProductById(Guid id);
}