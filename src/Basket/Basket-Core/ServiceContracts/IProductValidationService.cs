namespace Basket.Core.ServiceContracts;

public interface IProductValidationService
{
    Task ValidateBaskets(Guid registrationId);
}