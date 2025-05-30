namespace Basket.Components.Contracts;

public record InventoryReduced
{
    public Guid Id { get; init; }
    public DateTime RegistrationDate { get; init; }
}