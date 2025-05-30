namespace Catalog.Components;

public class Catalog
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    
    public List<CatalogItem> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);
    
    public string Customer { get; set; } = null!;
}