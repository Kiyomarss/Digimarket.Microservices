using ContactsManager.Core.Domain.IdentityEntities;

namespace Hesabdari_Core.ServiceContracts
{
    public interface IIdentityService
    {
        Task<ApplicationUser?> GetCurrentUserWithoutErrorAsync();

        Task<ApplicationUser> GetUserByIdAsync(string userId);
        
        Task<ApplicationUser> GetCurrentUserAsync();

        Task<bool> CurrentUserHasAnyRoleAsync(params string[] roleNames);

        Task<bool> CurrentUserHasRoleAsync(string roleName);

        Task<bool> UserHasRoleAsync(string userId, string roleName);

        bool IsUserLoggedIn();

        Task<IList<string>> GetCurrentUserRolesAsync();

        Task<bool> CurrentUserHasAllRolesAsync(params string[] roleNames);
        
        Task<bool> IsCurrentUserAdminAsync();

        Task<bool> IsUserAdminAsync(string userId);

        Task<bool> HasAccessAsync(string requiredPermission);
    }
}
