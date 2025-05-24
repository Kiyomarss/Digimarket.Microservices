namespace Ordering.Components.Consumers;

public record OrderValidated
{
    public Guid RegistrationId { get; init; }
}