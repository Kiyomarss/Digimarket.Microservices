using Basket.Core.Consumers;
using Basket.Core.ServiceContracts;
using MassTransit;

namespace Basket.Core.Services;

public class ProductValidationService :
    IProductValidationService
{
    readonly IPublishEndpoint _publishEndpoint;

    public ProductValidationService(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task ValidateBaskets(Guid registrationId)
    {
        await _publishEndpoint.Publish(new BasketValidated { RegistrationId = registrationId });
    }
}