namespace Shared.IntegrationEvents.Ordering;

public record OrderPaid 
{
    public Guid Id { get; set; }
}