namespace Ordering.Components.DTO;

public record OrderItemDto(Guid ProductId, string ProductName, int Quantity, int Price);
