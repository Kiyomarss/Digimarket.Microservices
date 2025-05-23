namespace Ordering.Components.Services;

public interface IOrderValidationService
{
    Task ValidateOrders(Guid registrationId);
}