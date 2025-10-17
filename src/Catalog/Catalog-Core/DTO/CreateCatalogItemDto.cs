namespace Catalog.Components.DTO;

public class CreateCatalogItemDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Stock { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
}
