using Catalog_Domain.Entities;

namespace Catalog_Domain.RepositoryContracts;

public interface IProductRepository
{
    Task<List<Product>> GetProductByIds(List<Guid> productIds, CancellationToken cancellationToken);
    
    Task AddProduct(Product product);
    
    Task<Product?> FindProductById(Guid id);
}