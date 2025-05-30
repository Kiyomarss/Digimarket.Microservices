namespace Basket.Components.Services;

public interface IProductValidationService
{
    Task ValidateBaskets(Guid registrationId);
}