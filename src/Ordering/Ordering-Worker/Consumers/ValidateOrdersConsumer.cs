using MassTransit;

namespace Ordering.Worker.Consumers;

public class ValidateOrdersConsumer :
    IConsumer<NotifyOrderConsumer>
{
    readonly ILogger<ValidateOrdersConsumer> _logger;

    public ValidateOrdersConsumer(ILogger<ValidateOrdersConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<NotifyOrderConsumer> context)
    {
        return Task.CompletedTask;
    }
}