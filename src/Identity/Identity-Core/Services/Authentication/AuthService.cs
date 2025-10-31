using ContactsManager.Core.Domain.IdentityEntities;
using Hesabdari_Core.DTO.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Hesabdari_Core.ServiceContracts;
using Hesabdari_Core.ServiceContracts.Base;
using Hesabdari_Core.ServiceContracts.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Hesabdari_Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IIdentityService _identityService;
        private readonly IConfiguration _configuration;
        private readonly IImageStorageService _imageStorageService;


        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IConfiguration configuration,
            IIdentityService identityService,
            IImageStorageService imageStorageService
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _identityService = identityService;
            _configuration = configuration;
            _imageStorageService = imageStorageService;
        }

        public async Task SignupAsync(SignupRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email, Email = request.Email, PersonName = request.PersonName
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                throw new InvalidOperationException("Signup Failed.");
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new InvalidOperationException("Invalid credentials.");

            var token = await _tokenService.GenerateJwtToken(user);

            var refreshToken = Guid.NewGuid().ToString();
            await _userManager.SetAuthenticationTokenAsync(user, "JWT", "RefreshToken", refreshToken);

            var isCurrentUserAdmin = await _identityService.IsUserAdminAsync(user.Id.ToString());
            
            return new LoginResult(token, new UserDetails(user.PersonName, user.Email, user.AvatarPath, isCurrentUserAdmin), refreshToken);
        }

        public async Task<UserDetails> GetCurrentUserDetailsAsync()
        {
            var user = await _identityService.GetCurrentUserAsync();
            
            var isCurrentUserAdmin = await _identityService.IsCurrentUserAdminAsync();
            
            return new UserDetails(user.PersonName, user.Email, user.AvatarPath, isCurrentUserAdmin);
        }
        
        public async Task<UserHeaderInfoResult> GetUserHeaderInfoAsync()
        {
            var user = await _identityService.GetCurrentUserAsync();
            
            return new UserHeaderInfoResult(user.PersonName, user.AvatarPath);
        }
        
        public async Task LogoutAsync()
        {
            var user = await _identityService.GetCurrentUserWithoutErrorAsync();
            if (user != null)
            {
                await _userManager.RemoveAuthenticationTokenAsync(user, "JWT", "RefreshToken");
            }

            await _signInManager.SignOutAsync();
        }
        
        public async Task<LoginResult> RefreshTokenAsync(string refreshToken, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            var savedToken = await _userManager.GetAuthenticationTokenAsync(user, "JWT", "RefreshToken");

            if (savedToken == null || savedToken != refreshToken)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            await _userManager.RemoveAuthenticationTokenAsync(user, "JWT", "RefreshToken");

            var newJwtToken = await _tokenService.GenerateJwtToken(user);

            var newRefreshToken = Guid.NewGuid().ToString();
            await _userManager.SetAuthenticationTokenAsync(user, "JWT", "RefreshToken", newRefreshToken);

            return new LoginResult(newJwtToken, new UserDetails(user.PersonName, user.Email, user.AvatarPath), newRefreshToken);
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await _identityService.GetCurrentUserAsync();

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            
            if (!result.Succeeded)
                throw new InvalidOperationException("ChangePassword Failed.");
        }

        public async Task ChangeUserNameAsync(string newUserName)
        {
            var user = await _identityService.GetCurrentUserAsync();

            var updateResult = await _userManager.SetUserNameAsync(user, newUserName);

            if (!updateResult.Succeeded)
                throw new InvalidOperationException("Failed to update UserName.");
        }

        public async Task UpdatePersonNameAsync(string newPersonName)
        {
            var user = await _identityService.GetCurrentUserAsync();

            user.PersonName = newPersonName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new InvalidOperationException("Failed to update PersonName.");
        }
        
        public async Task<string> UpdateAvatarAsync(Stream avatarStream)
        {
            var user = await _identityService.GetCurrentUserAsync();

            var avatarPath = await SaveNewAvatarFromStreamAsync(avatarStream);
            user.AvatarPath = avatarPath;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                if (File.Exists(Path.Combine("wwwroot", avatarPath)))
                    File.Delete(Path.Combine("wwwroot", avatarPath));

                throw new InvalidOperationException("Update Failed.");
            }

            return avatarPath;
        }
        
        public async Task DeleteAvatarAsync()
        {
            var user = await _identityService.GetCurrentUserAsync();

            await _imageStorageService.DeleteOldImagesAsync(user.AvatarPath);

            user.AvatarPath = null;

            await _userManager.UpdateAsync(user);
        }

        public virtual async Task<string> SaveNewAvatarFromStreamAsync(Stream fileStream)
        {
            var avatarFolderPath = Path.Combine("wwwroot", "avatars");
            Directory.CreateDirectory(avatarFolderPath);

            var fileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(avatarFolderPath, fileName);

            using (var image = await Image.LoadAsync(fileStream))
            {
                // تبدیل تصویر به JPEG
                await image.SaveAsJpegAsync(filePath, new JpegEncoder{ Quality = 90 });
            }

            return $"/avatars/{fileName}";
        }
        
        public async Task DeleteUserAsync(string userId)
        {
            var user = await _identityService.GetUserByIdAsync(userId);

            var result = await _userManager.DeleteAsync(user);
            
            if (!result.Succeeded)
                throw new InvalidOperationException("Delete Failed.");
        }
    }
}