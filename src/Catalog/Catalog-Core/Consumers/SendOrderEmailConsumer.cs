using Catalog.Core.DTO;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Catalog.Core.Consumers;

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