namespace Ordering.Core.ServiceContracts;

public interface IOrderValidationService
{
    Task ValidateOrders(Guid registrationId);
}