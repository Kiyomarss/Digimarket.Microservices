using Catalog.Worker.Events;
using MassTransit;

namespace Catalog.Worker.Consumers;

public class SendCatalogEmailConsumer :
    IConsumer<InventoryReduced>
{
    readonly ILogger<SendCatalogEmailConsumer> _logger;

    public SendCatalogEmailConsumer(ILogger<SendCatalogEmailConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<InventoryReduced> context)
    {
        _logger.LogInformation("Notifying Member {MemberId} that they registered for event {EventId} on {RegistrationDate}", context.Message.RegistrationDate);

        return Task.CompletedTask;
    }
}