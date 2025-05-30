namespace Catalog.Components;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<Category> Categories { get; set; } = new();
    public List<ProductImage> Images { get; set; } = new();
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public int AvailableStock { get; set; }
}