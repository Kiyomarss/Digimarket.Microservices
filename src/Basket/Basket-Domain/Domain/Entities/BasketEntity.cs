using MessagePack;

namespace Basket.Domain.Domain.Entities;

// عنوان BasketEntity به دلیل یکسان بودن namespace و عنوان این انتیتی قرار داد شده
[MessagePackObject]
public class BasketEntity
{
    [Key(0)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Key(1)]
    public Guid UserId { get; set; }

    [Key(2)]
    public List<BasketItem> Items { get; set; } = new();
}
