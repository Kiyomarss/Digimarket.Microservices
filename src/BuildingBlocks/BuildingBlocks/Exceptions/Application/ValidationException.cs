namespace BuildingBlocks.Exceptions.Application;

public sealed class ValidationException : ApplicationException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(
        string message,
        IDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }
}

