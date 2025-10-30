namespace Catalog.Application.DTO;

public record ProductDto(Guid Id, string Name, string Description, int Price, string? AttributesJson);
