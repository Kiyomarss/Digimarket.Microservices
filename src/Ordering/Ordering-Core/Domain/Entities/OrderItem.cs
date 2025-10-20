namespace Ordering.Components.Domain.Entities;

public class OrderItem
{
    internal OrderItem(Guid productId, string productName, int quantity, decimal price)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        Price = price;
    }

    public OrderItem() { }

    public Guid Id { get; set; }
    public Guid OrderId { get; set; }   
    public Order Order { get; set; } = default!;
    public Guid ProductId { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; } = default!;
    public decimal Price { get; set; } = default!;
}