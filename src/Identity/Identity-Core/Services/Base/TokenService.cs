using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Identity_Core.Domain.IdentityEntities;
using Identity_Core.ServiceContracts.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Identity_Core.Services.Base;

public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public TokenService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expirationHours = _configuration["Jwt:ExpirationHours"];
        var privateKeyPath = _configuration["Jwt:PrivateKeyPath"]; // مسیر فایل کلید خصوصی

        if (string.IsNullOrWhiteSpace(privateKeyPath) || !File.Exists(privateKeyPath))
            throw new FileNotFoundException("Private key file not found. Make sure Jwt:PrivateKeyPath is set.");

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("JWT configuration is incomplete.");

        var privateKeyPem = await File.ReadAllTextAsync(privateKeyPath);
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem); // به‌صورت خودکار PKCS#1 یا PKCS#8 را تشخیص می‌دهد

        var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("username", user.UserName ?? string.Empty)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // 🕒 زمان انقضا
        var expiration = DateTime.UtcNow.AddHours(Convert.ToDouble(expirationHours));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);
        
        return jwt;
    }
}