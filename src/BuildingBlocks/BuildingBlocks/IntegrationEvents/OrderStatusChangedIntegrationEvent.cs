namespace BuildingBlocks.IntegrationEvents;

public record OrderStatusChangedIntegrationEvent(Guid Id, string Status);
