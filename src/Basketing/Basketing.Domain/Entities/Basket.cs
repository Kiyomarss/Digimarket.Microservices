using MessagePack;

namespace Basketing.Domain.Entities;

[MessagePackObject]
public class Basket
{
    [Key(0)]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Key(1)]
    public Guid UserId { get; set; }

    [Key(2)]
    public IReadOnlyCollection<BasketItem> Items => _items;
    
    [IgnoreMember]
    private readonly List<BasketItem> _items = new();
}
