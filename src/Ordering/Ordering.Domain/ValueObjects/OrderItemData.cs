namespace Ordering_Domain.ValueObjects;

public record OrderItemData(
    Guid ProductId,
    long Price,
    int Quantity);