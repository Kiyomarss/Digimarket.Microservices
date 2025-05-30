namespace Catalog.Components.Contracts;

public record CatalogItemDto(Guid ProductId, string ProductName, int Quantity, decimal Price);
