namespace Ordering.Worker.StateMachines.Events;

public record AddEventAttendee
{
    public Guid RegistrationId { get; init; }
    public string Customer { get; init; } = null!;
}