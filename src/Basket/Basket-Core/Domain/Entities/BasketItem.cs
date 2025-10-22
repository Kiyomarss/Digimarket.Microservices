namespace Basket.Core.Domain.Entities;

public class BasketItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public Guid BasketId { get; set; }
    public BasketEntity Basket { get; set; }
}