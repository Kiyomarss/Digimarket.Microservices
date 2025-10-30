namespace Catalog.Worker.Events;

public record CatalogInitiated
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public string Customer { get; init; } = null!;
}