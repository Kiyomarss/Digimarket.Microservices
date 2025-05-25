namespace Ordering.Components;

public class Order
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);
    
    public string Customer { get; set; } = null!;
}