using Catalog_Domain.Entities;
using Catalog.Application.Products.Queries;

namespace Catalog.Application.RepositoryContracts;

public interface IProductRepository
{
    Task<IEnumerable<ProductDto>> GetProductByIds(IEnumerable<Guid> productIds, CancellationToken cancellationToken);
    
    Task AddProduct(Product product);
    
    Task<Product?> FindProductById(Guid id);
}