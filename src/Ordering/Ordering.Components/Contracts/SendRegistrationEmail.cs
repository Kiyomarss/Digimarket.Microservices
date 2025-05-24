namespace Ordering.Components.Contracts;

public record SendRegistrationEmail
{
    public Guid RegistrationId { get; init; }
    public DateTime RegistrationDate { get; init; }
    public string Customer { get; init; } = null!;
    public string EventId { get; init; } = null!;
}