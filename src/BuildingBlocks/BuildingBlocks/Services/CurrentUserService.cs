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
        if (user?.Identity == null || !user.Identity.IsAuthenticated)
            return null;

        // در JWT ما claim مربوط به شناسه کاربر را با نام "sub" قرار داده‌ایم
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
                          ?? user.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim == null)
            return null;

        return Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }

    public string? GetUserName()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirst("username")?.Value;
    }

    public string? GetEmail()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirst(ClaimTypes.Email)?.Value
               ?? user?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
    
    public Metadata GetAuthorizationHeaders()
    {
        var headers = new Metadata();
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

        if (!string.IsNullOrWhiteSpace(token))
        {
            // اگر شامل Bearer نیست، اضافه کن
            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = "Bearer " + token;

            headers.Add("Authorization", token);
        }

        return headers;
    }
}