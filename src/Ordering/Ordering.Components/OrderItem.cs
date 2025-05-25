using System.Text.Json.Serialization;

namespace Ordering.Components;

public class OrderItem
{
    [JsonConstructor]
    internal OrderItem(Guid productId, string productName, int quantity, decimal price)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        Price = price;
    }

    public Guid Id { get; set; }
    public Guid OrderId { get; set; }   
    public Guid ProductId { get; private set; } = default!;
    public string ProductName { get; private set; } = default!;
    public int Quantity { get; private set; } = default!;
    public decimal Price { get; private set; } = default!;
    public Order Order { get; set; } = default!;
}