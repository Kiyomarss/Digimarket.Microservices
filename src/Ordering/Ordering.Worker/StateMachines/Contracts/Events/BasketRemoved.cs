namespace Ordering.Worker.StateMachines.Contracts.Events;

public record BasketRemoved
{
    public Guid Id { get; init; }
}