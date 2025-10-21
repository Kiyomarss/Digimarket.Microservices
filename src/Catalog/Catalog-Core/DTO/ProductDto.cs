namespace Catalog.Components.DTO;

public record ProductDto(Guid Id, string Name, string Description, int Stock, string? AttributesJson);
