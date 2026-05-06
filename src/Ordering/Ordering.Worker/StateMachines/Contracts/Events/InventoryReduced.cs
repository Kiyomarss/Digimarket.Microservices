namespace Ordering.Worker.StateMachines.Contracts.Events;

public record InventoryReduced
{
    public Guid Id { get; init; }
    public DateTime RegistrationDate { get; init; }
}