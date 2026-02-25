using BuildingBlocks.Exceptions.Domain;

namespace Catalog_Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Description { get; set; }
    public int Stock { get; private set; }
    public long Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? AttributesJson { get; set; }
    
    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive");

        if (Stock < quantity)
            throw new DomainException("Not enough stock");

        Stock -= quantity;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("Quantity must be positive");

        Stock += quantity;
    }
}
