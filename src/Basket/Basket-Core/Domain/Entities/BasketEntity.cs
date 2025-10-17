namespace Basket.Core.Domain.Entities;

// عنوان BasketEntity به دلیل یکسان بودن namespace و عنوان این انتیتی قرار داد شده
public class BasketEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public List<BasketItem> Items { get; set; } = new();
}
