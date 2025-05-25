namespace Ordering.Components.Contracts;

public record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal Price);
