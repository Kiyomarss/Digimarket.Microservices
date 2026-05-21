using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Extensions;

public static class JwtAuthenticationExtension
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authority = configuration["Jwt:Authority"];
        var audience = configuration["Jwt:Audience"];
        var issuer = configuration["Jwt:ValidIssuer"];

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = true,
                    ValidAudiences = [audience],
                    
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,

                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
                options.MapInboundClaims = false; // هیچ کلیم ورودی را تغییر نده و به همان شکلی که در توکن آمده (مثلاً "role") به من تحویل بده
            });

        services.AddAuthorization();

        return services;
    }
}