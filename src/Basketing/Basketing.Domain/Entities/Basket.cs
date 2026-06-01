using MessagePack;

namespace Basketing.Domain.Entities;

// عنوان Basket به دلیل یکسان بودن namespace و عنوان این انتیتی قرار داد شده
[MessagePackObject]
public class Basket
{
    [Key(0)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Key(1)]
    public Guid UserId { get; set; }

    [Key(2)]
    public IReadOnlyCollection<BasketItem> Items => _items;
    
    [IgnoreMember]
    private readonly List<BasketItem> _items = new();
}
