namespace Basket.Components;

public class BasketItem
{
    public Guid Id { get; set; }

    public Guid CatalogId { get; set; }

    public int Quantity { get; set; }

    public Guid BasketId { get; set; }
    public Basket Basket { get; set; }
}