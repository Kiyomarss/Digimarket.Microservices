namespace Catalog.Components.Services;

public interface IProductValidationService
{
    Task ValidateCatalogs(Guid registrationId);
}