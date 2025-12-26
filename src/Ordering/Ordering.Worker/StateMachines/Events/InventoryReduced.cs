namespace Ordering.Worker.StateMachines.Events;

public record InventoryReduced
{
    public Guid Id { get; init; }
    public DateTime RegistrationDate { get; init; }
}