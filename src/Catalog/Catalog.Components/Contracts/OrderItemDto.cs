namespace Catalog.Components.Contracts;

public record CatalogItemDto(Guid Id, string Name, string Description, int Stock, string? AttributesJson);
