using System.Text.Json.Serialization;
using MessagePack;

namespace Basket.Domain.Domain.Entities;

[MessagePackObject]
public class BasketItem
{
    [Key(0)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Key(1)]
    public Guid ProductId { get; set; }

    [Key(2)]
    public int Quantity { get; set; }

    [Key(3)]
    public Guid BasketId { get; set; }
    
    [IgnoreMember]
    [JsonIgnore]
    public BasketEntity Basket { get; set; }
}