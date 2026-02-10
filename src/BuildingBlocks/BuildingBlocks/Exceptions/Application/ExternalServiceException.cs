namespace BuildingBlocks.Exceptions.Application;

public class ExternalServiceException : ApplicationException
{
    public ExternalServiceException(string message) : base(message) { }
}
