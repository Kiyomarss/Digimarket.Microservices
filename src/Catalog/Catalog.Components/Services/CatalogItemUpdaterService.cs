using System.Text.Json;
using Catalog.Components.Contracts;
using Catalog.Components.Repositories;

namespace Catalog.Components;

public class CatalogItemUpdaterService : ICatalogItemUpdaterService
{
    readonly ICatalogItemRepository _catalogItemRepository;

    public CatalogItemUpdaterService(ICatalogItemRepository catalogItemRepository)
    {
        _catalogItemRepository = catalogItemRepository;
    }

    public async Task AddItem(Guid catalogId, int quantity)
    {
    }
    
    public async Task AddCatalogItem(CreateCatalogItemDto dto)
    {
        var catalogItem = new CatalogItem
        {
            Name = dto.Name,
            Description = dto.Description,
            Stock = dto.Stock,
            CreatedAt = DateTime.UtcNow,
            AttributesJson = JsonSerializer.Serialize(dto.Attributes)
        };

        await _catalogItemRepository.AddCatalogItem(catalogItem);
    }
    
    public async Task RemoveItem(Guid id)
    {
        var entity = await _catalogItemRepository.FindCatalogItemById(id);

        if (entity == null)
            throw new Exception("FindAsync not found for user.");

        var deleted = await _catalogItemRepository.DeleteCatalogItem(id);
        
        if (!deleted)
            throw new Exception($"No CatalogItem found with Id = {id}");
    }
}