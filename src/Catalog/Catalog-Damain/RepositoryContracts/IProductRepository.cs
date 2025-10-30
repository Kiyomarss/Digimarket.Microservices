using Catalog_Damain.Entities;

namespace Catalog_Damain.RepositoryContracts;

public interface IProductRepository
{
    Task<List<Product>> GetProductByIds(List<Guid> productIds);
    
    Task AddProduct(Product product);
    
    Task<Product?> FindProductById(Guid id);

    Task<bool> DeleteProduct(Guid id);
}