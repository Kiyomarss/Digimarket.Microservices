using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Components.Contracts;
using Ordering.Components.Services;

namespace Ordering.Components.Consumers;

public class ValidateOrdersConsumer :
    IConsumer<PaymentValidated>
{
    readonly ILogger<ValidateOrdersConsumer> _logger;
    readonly IOrderValidationService _validationService;

    public ValidateOrdersConsumer(ILogger<ValidateOrdersConsumer> logger, IOrderValidationService validationService)
    {
        _logger = logger;
        _validationService = validationService;
    }

    public async Task Consume(ConsumeContext<PaymentValidated> context)
    {
        await _validationService.ValidateOrders(context.Message.Id);
    }
}