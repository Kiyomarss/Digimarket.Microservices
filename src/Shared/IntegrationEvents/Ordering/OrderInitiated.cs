namespace Shared.IntegrationEvents.Ordering;


public sealed record OrderInitiated(
    Guid Id,
    Guid UserId,
    IReadOnlyCollection<OrderInitiated.OrderItemDto> Items,
    DateTime Date)
{
    public sealed record OrderItemDto(
        Guid ProductId,
        int Quantity);
}
    