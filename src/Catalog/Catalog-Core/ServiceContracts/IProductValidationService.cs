namespace Catalog.Core.ServiceContracts;

public interface IProductValidationService
{
    Task ValidateCatalogs(Guid registrationId);
}