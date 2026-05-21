using System.Security.Claims;
using Identity.Application.RepositoryContracts;
using Identity.Application.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Identity.Api.Controllers;

[ApiController]
[Route("connect")]
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

    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();

        if (request is null)
            throw new InvalidOperationException("OpenIddict request cannot be retrieved.");

        if (request.IsPasswordGrantType())
        {
            var user = await _userRepository.GetByEmailAsync(
                                                             request.Username!,
                                                             HttpContext.RequestAborted);

            if (user is null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var isValidPassword = _passwordHasher.VerifyPassword(
                                                                 request.Password!,
                                                                 user.PasswordHash);

            if (!isValidPassword)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(
                                              OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());
            identity.AddClaim(OpenIddictConstants.Claims.Email, user.Email);
            identity.AddClaim(OpenIddictConstants.Claims.Name, user.Email);

            foreach (var role in user.Roles)
            {
                identity.AddClaim(OpenIddictConstants.Claims.Role, role.Role.Name);
            }
            
            foreach (var claim in user.Claims)
            {
                identity.AddClaim(claim.Type, claim.Value);
            }
            
            var principal = new ClaimsPrincipal(identity);

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
            }

            var scopes = request.GetScopes();

            principal.SetScopes(scopes);

            var audiences = scopes
                            .Where(s => ScopeAudienceMap.ContainsKey(s))
                            .Select(s => ScopeAudienceMap[s])
                            .Distinct()
                            .ToList();

            principal.SetResources(audiences);
            principal.SetAudiences(audiences);
            
            return SignIn(
                          principal,
                          OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotImplementedException("Grant type is not supported.");
    }
    
    private static readonly Dictionary<string, string> ScopeAudienceMap =
        new()
        {
            ["catalog"] = "catalog-api",
            ["basket"] = "basket-api",
            ["ordering"] = "ordering-api",
            ["identity"] = "identity-api"
        };
}