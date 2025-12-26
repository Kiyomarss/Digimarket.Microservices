namespace Ordering.Worker.Consumers;

public record OrderValidated
{
    public Guid RegistrationId { get; init; }
}