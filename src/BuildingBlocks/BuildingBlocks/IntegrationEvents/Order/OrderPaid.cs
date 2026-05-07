namespace BuildingBlocks.IntegrationEvents.Order;

public record OrderPaid 
{
    public Guid Id { get; set; }
}