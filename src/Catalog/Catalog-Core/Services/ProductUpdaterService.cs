using System.Text.Json;
using Catalog.Components.DTO;
using Catalog.Components.Repositories;

namespace Catalog.Components;

public class ProductUpdaterService : IProductUpdaterService
{
    readonly IProductRepository _productRepository;

    public ProductUpdaterService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task AddItem(Guid catalogId, int quantity)
    {
    }
    
    public async Task AddProduct(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Stock = dto.Stock,
            CreatedAt = DateTime.UtcNow,
            AttributesJson = JsonSerializer.Serialize(dto.Attributes)
        };

        await _productRepository.AddProduct(product);
    }
    
    public async Task RemoveItem(Guid id)
    {
        var entity = await _productRepository.FindProductById(id);

        if (entity == null)
            throw new Exception("FindAsync not found for user.");

        var deleted = await _productRepository.DeleteProduct(id);
        
        if (!deleted)
            throw new Exception($"No Product found with Id = {id}");
    }
}