using System.ComponentModel.DataAnnotations;

namespace Ordering_Domain.Domain.Entities;

public class OrderItem
{
    public OrderItem(Guid productId, int quantity, int price)
    {
        ProductId = productId;
        Quantity = quantity;
        Price = price;
    }

    public OrderItem() { }

    public Guid Id { get; protected set; } = Guid.CreateVersion7();
    public Guid OrderId { get; set; }   
    public Order Order { get; set; } = default!;
    public Guid ProductId { get; set; } = default!;
    public int Quantity { get; set; } = default!;
    
    public long Price { get; set; } = default!;
    
    public static OrderItem Create(
        Guid orderId,
        Guid productId,
        long price,
        int quantity)
    {
        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            Price = price,
            Quantity = quantity
        };
    }
}