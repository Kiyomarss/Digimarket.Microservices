namespace Basket.Worker.Events;

public record BasketRemoved
{
    public Guid Id { get; init; }
}