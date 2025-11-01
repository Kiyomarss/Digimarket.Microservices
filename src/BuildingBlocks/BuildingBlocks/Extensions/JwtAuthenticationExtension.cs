using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Extensions;

public static class JwtAuthenticationExtension
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // مسیر کلید عمومی PEM
        var publicKeyPath = configuration["Jwt:PublicKeyPath"];

        if (string.IsNullOrWhiteSpace(publicKeyPath) || !File.Exists(publicKeyPath))
            throw new FileNotFoundException($"Public key not found at: {publicKeyPath}");

        // خواندن محتوای PEM
        var publicKeyPem = File.ReadAllText(publicKeyPath);

        // ساخت کلید RSA
        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        var rsaKey = new RsaSecurityKey(rsa);

        // اضافه کردن JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = configuration["Jwt:Audience"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = rsaKey
                    };
                });

        return services;
    }
}