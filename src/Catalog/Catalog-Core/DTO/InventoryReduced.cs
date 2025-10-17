namespace Catalog.Components.DTO;

public record InventoryReduced
{
    public Guid Id { get; init; }
    public DateTime RegistrationDate { get; init; }
}