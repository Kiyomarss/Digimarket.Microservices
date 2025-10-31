using ContactsManager.Core.Domain.IdentityEntities;

namespace Hesabdari_Core.ServiceContracts.Base;

public interface ITokenService
{
    Task<string> GenerateJwtToken(ApplicationUser user);
}