namespace BuildingBlocks.IntegrationEvents.Order;

public record OrderCancelledAfterPayment(Guid Id, IEnumerable<OrderCancelledAfterPayment.ProductItemsDto> Items)
{
    public sealed record ProductItemsDto(
        Guid ProductId,
        int Quantity);
}
