using Basket.Core.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Basket.Core.Consumers;

public class SendBasketEmailConsumer :
    IConsumer<InventoryReduced>
{
    readonly ILogger<SendBasketEmailConsumer> _logger;

    public SendBasketEmailConsumer(ILogger<SendBasketEmailConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<InventoryReduced> context)
    {
        _logger.LogInformation("Notifying Member {MemberId} that they registered for event {EventId} on {RegistrationDate}", context.Message.RegistrationDate);

        return Task.CompletedTask;
    }
}