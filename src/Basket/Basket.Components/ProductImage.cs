namespace Basket.Components;

public class ProductImage
{
    public Guid Id { get; set; }
    
    public Guid ProductId { get; set; }
    public ShoppingCart ShoppingCart { get; set; } = default!;
    
    public string Url { get; set; } = default!;
    public string? AltText { get; set; }
}