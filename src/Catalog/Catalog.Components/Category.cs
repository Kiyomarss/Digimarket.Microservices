namespace Catalog.Components;

public class Category
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public Product Product { get; set; } = default!;

}