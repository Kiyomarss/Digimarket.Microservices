namespace BuildingBlocks.Exceptions.Application;

public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message) : base(message) {}
}
