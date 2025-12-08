namespace Shared.IntegrationEvents.Ordering;

public record OrderStatusChanged 
{
    public Guid Id { get; set; }

    public string State { get; set; }
}