using Catalog.Core.Consumers;
using Catalog.Core.ServiceContracts;
using MassTransit;

namespace Catalog.Core.Services;

public class ProductValidationService :
    IProductValidationService
{
    readonly IPublishEndpoint _publishEndpoint;

    public ProductValidationService(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task ValidateCatalogs(Guid registrationId)
    {
        await _publishEndpoint.Publish(new CatalogValidated { RegistrationId = registrationId });
    }
}