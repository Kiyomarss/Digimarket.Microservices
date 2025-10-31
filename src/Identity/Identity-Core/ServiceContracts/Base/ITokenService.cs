using Identity_Core.Domain.IdentityEntities;

namespace Identity_Core.ServiceContracts.Base;

public interface ITokenService
{
    Task<string> GenerateJwtToken(ApplicationUser user);
}