namespace Ordering.Core.Consumers;

public record OrderValidated
{
    public Guid RegistrationId { get; init; }
}