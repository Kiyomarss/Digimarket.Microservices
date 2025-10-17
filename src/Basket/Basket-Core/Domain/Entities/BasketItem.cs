namespace Basket.Core.Domain.Entities;

public class BasketItem
{
    public Guid Id { get; set; }

    public Guid CatalogId { get; set; }

    public int Quantity { get; set; }

    public Guid BasketId { get; set; }
    public BasketEntity Basket { get; set; }
}