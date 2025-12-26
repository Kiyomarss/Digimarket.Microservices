namespace Catalog.Worker.Events;

public record CatalogRemoved
{
    public Guid Id { get; init; }
}