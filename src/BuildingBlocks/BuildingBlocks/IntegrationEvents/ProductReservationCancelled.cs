namespace BuildingBlocks.IntegrationEvents;

public record ProductReservationCancelled(IEnumerable<ProductReservationCancelled.ProductItemsDto> Items)
{
    public sealed record ProductItemsDto(
        Guid ProductId,
        int Quantity);
}
