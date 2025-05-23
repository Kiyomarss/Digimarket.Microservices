namespace Ordering.Components;

public class OrderWrapper
{
    public Guid Id { get; set; }

    public Order Data { get; set; } = null!;
}