using BuildingBlocks.Common.Messaging;

namespace BuildingBlocks.IntegrationEvents
{
    public record OrderCreatedIntegrationEvent(Guid OrderId) : IntegrationEvent;
}
