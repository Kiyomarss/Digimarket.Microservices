namespace Catalog.Components;

[Serializable]
public class DuplicateCatalogException :
    Exception
{
    public DuplicateCatalogException()
    {
    }

    public DuplicateCatalogException(string? message) : base(message)
    {
    }

    public DuplicateCatalogException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}