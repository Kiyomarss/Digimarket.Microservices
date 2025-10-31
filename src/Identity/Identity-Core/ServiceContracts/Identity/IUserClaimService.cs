using System.Security.Claims;

namespace Identity_Core.ServiceContracts.Identity
{
    public interface IUserClaimService
    {
        Task<bool> AddClaimToUserAsync(string userId, string claimType, string claimValue);
        Task<bool> RemoveClaimFromUserAsync(string userId, string claimType, string claimValue);
        Task<IList<Claim>> GetClaimsByUserAsync(string userId);
        Task<bool> UserHasClaimAsync(string userId, string claimType, string claimValue);
    }
}