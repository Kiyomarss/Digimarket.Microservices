using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Core.ServiceContracts;

namespace Ordering.Core.Consumers;

public class ValidateOrdersConsumer :
    IConsumer<NotifyOrderConsumer>
{
    readonly ILogger<ValidateOrdersConsumer> _logger;
    readonly IOrderValidationService _validationService;

    public ValidateOrdersConsumer(ILogger<ValidateOrdersConsumer> logger, IOrderValidationService validationService)
    {
        _logger = logger;
        _validationService = validationService;
    }

    public async Task Consume(ConsumeContext<NotifyOrderConsumer> context)
    {
        await _validationService.ValidateOrders(new Guid());
    }
}