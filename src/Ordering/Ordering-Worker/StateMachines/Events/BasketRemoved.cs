namespace Ordering.Worker.StateMachines.Events;

public record BasketRemoved
{
    public Guid Id { get; init; }
}