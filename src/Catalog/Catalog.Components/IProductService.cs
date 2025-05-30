using Catalog.Components.Contracts;

namespace Catalog.Components;

public interface IProductService
{
    Task<Product> SubmitCatalogs();
}