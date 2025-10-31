using System.Security.Claims;

namespace Identity_Core.ServiceContracts.RoleManagement
{
    public interface IRoleClaimService
    {
        Task<bool> AddClaimToRoleAsync(string roleName, string claimType, string claimValue);

        Task<bool> RemoveClaimFromRoleAsync(string roleName, string claimType, string claimValue);

        Task<IList<Claim>> GetClaimsByRoleAsync(string roleName);

        Task<bool> RoleHasClaimAsync(string roleName, string claimType, string claimValue);
    }
}