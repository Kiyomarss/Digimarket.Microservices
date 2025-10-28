using MassTransit;
using Ordering.Worker.StateMachines.Events;

namespace Ordering.Worker.Consumers;

public class PaymentCompletedConsumer :
    IConsumer<PaymentCompleted>
{
    readonly ILogger<PaymentCompletedConsumer> _logger;

    public PaymentCompletedConsumer(ILogger<PaymentCompletedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<PaymentCompleted> context)
    {
        Console.WriteLine($"PaymentCompleted received: {context.Message.CorrelationId}");

        return Task.CompletedTask;
    }
}