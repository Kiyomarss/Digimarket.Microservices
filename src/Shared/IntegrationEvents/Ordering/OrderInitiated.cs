namespace Shared.IntegrationEvents.Ordering;

public record OrderInitiated
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public string Customer { get; init; } = null!;
}