namespace Ordering.Core.DTO;

public record AddEventAttendee
{
    public Guid RegistrationId { get; init; }
    public string Customer { get; init; } = null!;
}