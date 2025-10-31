using Hesabdari_Core.DTO.Auth;

namespace Hesabdari_Core.ServiceContracts;

public interface IAuthService
{
    Task SignupAsync(SignupRequest request);

    Task<LoginResult> LoginAsync(LoginRequest request);
    
    Task<UserDetails> GetCurrentUserDetailsAsync();
    
    Task LogoutAsync();

    Task<LoginResult> RefreshTokenAsync(string refreshToken, string email);
    
    Task<UserHeaderInfoResult> GetUserHeaderInfoAsync();
    
    Task ChangePasswordAsync(ChangePasswordRequest request);

    Task ChangeUserNameAsync(string newUserName);

    Task UpdatePersonNameAsync(string newPersonName);
    
    Task<string> UpdateAvatarAsync(Stream avatarStream);

    Task DeleteAvatarAsync();

    Task DeleteUserAsync(string userId);
}