namespace BuildingBlocks.Exceptions.Application;

public class NotFoundException : ApplicationException
{
    public NotFoundException(string message) : base(message) { }
}
