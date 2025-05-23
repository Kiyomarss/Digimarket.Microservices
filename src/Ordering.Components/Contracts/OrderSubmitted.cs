namespace Ordering.Components.Contracts;

public record OrderSubmitted
{
    public Guid RegistrationId { get; init; }
    public DateTime RegistrationDate { get; init; }
    public string Customer { get; init; } = null!;
}