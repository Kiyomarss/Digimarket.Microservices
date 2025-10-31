using Identity_Core.Domain.IdentityEntities;
using Identity_Core.DTO;
using Identity_Core.DTO.Auth;
using Identity_Core.ServiceContracts.Base;
using Identity_Core.ServiceContracts.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity_UI.Controllers;

public class ExternalLoginController : BaseController
{
    private readonly IUserLoginService _userLoginService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public ExternalLoginController(IUserLoginService userLoginService, UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userLoginService = userLoginService;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveExternalLogin([FromBody] RemoveExternalLoginRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        if (user == null)
            return NotFound(new MessageResponse("User not found."));

        var result = await _userLoginService.RemoveLoginAsync(user, request.LoginProvider, request.ProviderKey);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new MessageResponse("External login removed successfully."));
    }

    [HttpGet("logins/{userId}")]
    public async Task<IActionResult> GetExternalLogins(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new MessageResponse("User not found."));

        var logins = await _userLoginService.GetLoginsAsync(user);

        return Ok(logins);
    }
    
    [HttpPost("external-login")]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginRequest request)
    {
        var loginInfo = new UserLoginInfo(request.LoginProvider, request.ProviderKey, request.ProviderDisplayName);
        var user = await _userLoginService.ExternalLoginAsync(loginInfo, request.EmailFromProvider);

        if (user == null)
            return Unauthorized(new MessageResponse("External login failed: Unable to create or find user."));

        var token = await _tokenService.GenerateJwtToken(user);
        return Ok(new LoginResult(token));
    }
}

public record RemoveExternalLoginRequest(string UserId, string LoginProvider, string ProviderKey);

public record ExternalLoginRequest(string LoginProvider, string ProviderKey, string ProviderDisplayName, String? EmailFromProvider);