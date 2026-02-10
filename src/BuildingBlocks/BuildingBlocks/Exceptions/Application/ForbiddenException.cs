namespace BuildingBlocks.Exceptions.Application;

public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message) : base(message) { }
}
