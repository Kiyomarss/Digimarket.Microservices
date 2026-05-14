using System.Security.Claims;
using Identity.Application.RepositoryContracts;
using Identity.Application.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Identity.Api.Controllers;

[ApiController]
public class AuthorizationController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasher;

    public AuthorizationController(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request =
            HttpContext.GetOpenIddictServerRequest();

        if (request is null)
            throw new InvalidOperationException(
                "OpenIddict request cannot be retrieved.");

        if (request.IsPasswordGrantType())
        {
            var user = await _userRepository
                .GetByEmailAsync(
                    request.Username!,
                    HttpContext.RequestAborted);

            if (user is null)
            {
                return Forbid(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var isValidPassword =
                _passwordHasher.VerifyPassword(
                    request.Password!,
                    user.PasswordHash);

            if (!isValidPassword)
            {
                return Forbid(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var identity = new ClaimsIdentity(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            identity.AddClaim(
                OpenIddictConstants.Claims.Subject,
                user.Id.ToString());

            identity.AddClaim(
                OpenIddictConstants.Claims.Email,
                user.Email);

            identity.AddClaim(
                OpenIddictConstants.Claims.Name,
                user.Email);

            var principal = new ClaimsPrincipal(identity);

            principal.SetScopes(new[]
            {
                OpenIddictConstants.Scopes.OpenId,
                OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Scopes.Profile,
                OpenIddictConstants.Scopes.OfflineAccess, 
                "identity", 
                "basket",
                "catalog",
                "ordering"
            });

            return SignIn(
                principal,
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotImplementedException(
            "Grant type is not supported.");
    }
}