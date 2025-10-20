using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Components.DTO;

namespace Ordering.Components.Consumers;

public class SendOrderEmailConsumer :
    IConsumer<InventoryReduced>
{
    readonly ILogger<SendOrderEmailConsumer> _logger;

    public SendOrderEmailConsumer(ILogger<SendOrderEmailConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<InventoryReduced> context)
    {
        _logger.LogInformation("Notifying Member {MemberId} that they registered for event {EventId} on {RegistrationDate}", context.Message.RegistrationDate);

        return Task.CompletedTask;
    }
}