namespace Catalog.Components;

public class ProductImage
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Url { get; set; } = default!;
    public string? AltText { get; set; }
    public Product Product { get; set; } = default!;
}