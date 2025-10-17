namespace Catalog.Components.DTO;

public record CatalogItemDto(Guid Id, string Name, string Description, int Stock, string? AttributesJson);
