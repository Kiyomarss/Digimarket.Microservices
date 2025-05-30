namespace Catalog.Components;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
}