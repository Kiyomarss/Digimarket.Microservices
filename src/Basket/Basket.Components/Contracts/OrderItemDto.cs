namespace Basket.Components.Contracts;

public record BasketItemDto(Guid ProductId, string ProductName, int Quantity, decimal Price);
