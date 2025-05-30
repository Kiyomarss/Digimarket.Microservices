using System.Text.Json.Serialization;

namespace Catalog.Components;

public class CatalogItem
{
    internal CatalogItem(Guid productId, string productName, int quantity, decimal price)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        Price = price;
    }

    public CatalogItem() { }

    public Guid Id { get; set; }
    public Guid CatalogId { get; set; }   
    public Guid ProductId { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; } = default!;
    public decimal Price { get; set; } = default!;
    public Catalog Catalog { get; set; } = default!;
}