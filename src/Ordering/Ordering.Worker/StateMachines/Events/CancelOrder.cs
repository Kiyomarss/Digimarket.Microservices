namespace Ordering.Worker.StateMachines.Events;

public record CancelOrder(Guid Id, IEnumerable<CancelOrder.OrderItemDto> Items)
{
    public sealed record OrderItemDto(
        Guid ProductId,
        int Quantity);
}
