using System.Security.Claims;

namespace Hesabdari_Core.ServiceContracts
{
    public interface IUserClaimService
    {
        Task<bool> AddClaimToUserAsync(string userId, string claimType, string claimValue);
        Task<bool> RemoveClaimFromUserAsync(string userId, string claimType, string claimValue);
        Task<IList<Claim>> GetClaimsByUserAsync(string userId);
        Task<bool> UserHasClaimAsync(string userId, string claimType, string claimValue);
    }
}