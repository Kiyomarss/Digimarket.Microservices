using Catalog.Components.Contracts;

namespace Catalog.Components;

public interface ICatalogService
{
    Task<Catalog> SubmitCatalogs(List<CatalogItemDto> CatalogItemDtos);
}