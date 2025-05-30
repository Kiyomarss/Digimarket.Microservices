namespace Catalog.Components;

[Serializable]
public class DuplicateProductException :
    Exception
{
    public DuplicateProductException()
    {
    }

    public DuplicateProductException(string? message) : base(message)
    {
    }

    public DuplicateProductException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}