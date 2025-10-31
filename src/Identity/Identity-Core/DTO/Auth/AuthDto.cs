using Microsoft.AspNetCore.Http;

namespace Hesabdari_Core.DTO.Auth;

public record SignupRequest(string PersonName, string Email, string Password);

public record LoginResult(string Token = "", UserDetails User = null, string RefreshToken = "");

public record LoginRequest(string Email, string Password);

public record UserHeaderInfoResult(string? personName, string? avatarPath);

public record UserDetails(string? PersonName,string Email, string? AvatarPath, bool IsAdmin = false);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public class UpdateAvatarRequest
{
    public required IFormFile Image { get; set; }
}

public record UpdatePersonNameRequest(string NewPersonName);

public record UserDto(string FullName, string Email, string? AvatarPath);