using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity is not { IsAuthenticated: true })
            return null;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
                          ?? user.FindFirst(JwtRegisteredClaimNames.Sub);

        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) 
                   ? userId 
                   : null;
    }

    public Task<Guid> GetRequiredUserId() =>
        Task.FromResult(GetUserId() 
                        ?? throw new UnauthorizedAccessException("User is not authenticated or UserId is missing."));

    public string? GetUserName() =>
        _httpContextAccessor.HttpContext?.User?
            .FindFirst("username")?.Value;

    public string? GetEmail() =>
        _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.Email)?.Value
        ?? _httpContextAccessor.HttpContext?.User?
            .FindFirst(JwtRegisteredClaimNames.Email)?.Value;

    public bool IsAuthenticated() =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public Metadata GetAuthorizationHeaders()
    {
        var headers = new Metadata();
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (!string.IsNullOrWhiteSpace(authHeader))
        {
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                authHeader = "Bearer " + authHeader.Trim();

            headers.Add("Authorization", authHeader);
        }

        return headers;
    }
}