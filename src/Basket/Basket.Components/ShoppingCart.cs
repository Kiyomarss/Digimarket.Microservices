namespace Basket.Components;

public class ShoppingCart
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public int AvailableStock { get; set; }

    public List<ProductImage> Images { get; set; } = new();
}
