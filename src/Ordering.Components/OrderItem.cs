namespace Ordering.Components;

public class OrderItem
{
    internal OrderItem(Guid productId, string productName, int quantity, decimal price)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        Price = price;
    }

    public Guid ProductId { get; private set; } = default!;
    public string ProductName { get; private set; } = default!;
    public int Quantity { get; private set; } = default!;
    public decimal Price { get; private set; } = default!;
}