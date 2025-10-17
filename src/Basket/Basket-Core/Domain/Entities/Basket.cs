namespace Basket.Core;

public class Basket
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public List<BasketItem> Items { get; set; } = new();
}
