using Catalog.Components.DTO;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Catalog.Components.Consumers;

public class NotifyCatalogConsumer :
    IConsumer<CatalogInitiated>
{
    readonly ILogger<NotifyCatalogConsumer> _logger;

    public NotifyCatalogConsumer(ILogger<NotifyCatalogConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CatalogInitiated> context)
    {
        _logger.LogInformation("Member {MemberId} registered for event {EventId} on {RegistrationDate}", context.Message.Customer,
            context.Message.Date);

        return Task.CompletedTask;
    }
}