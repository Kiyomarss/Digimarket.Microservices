namespace Catalog.Components;

public interface ICatalogItemUpdaterService
{
    Task AddItem(Guid catalogId, int quantity);

    Task RemoveItem(Guid id);
}