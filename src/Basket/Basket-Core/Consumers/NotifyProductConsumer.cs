using Basket.Core.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Basket.Core.Consumers;

public class NotifyBasketConsumer :
    IConsumer<BasketInitiated>
{
    readonly ILogger<NotifyBasketConsumer> _logger;

    public NotifyBasketConsumer(ILogger<NotifyBasketConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<BasketInitiated> context)
    {
        _logger.LogInformation("Member {MemberId} registered for event {EventId} on {RegistrationDate}", context.Message.Customer,
            context.Message.Date);

        return Task.CompletedTask;
    }
}