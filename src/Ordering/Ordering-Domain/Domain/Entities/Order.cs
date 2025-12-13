namespace Ordering_Domain.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; }
    public string State { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public long TotalPrice => Items.Sum(x => x.Price * x.Quantity);
    
    public Guid UserId { get; set; }
    public string Customer { get; set; } = null!;
}