namespace Ordering_Domain.Domain.Entities;

public class OrderItem
{
    public OrderItem(Guid productId, string productName, int quantity, int price)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        Price = price;
    }

    public OrderItem() { }

    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }   
    public Order Order { get; set; } = default!;
    public Guid ProductId { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; } = default!;
    public long Price { get; set; } = default!;
}