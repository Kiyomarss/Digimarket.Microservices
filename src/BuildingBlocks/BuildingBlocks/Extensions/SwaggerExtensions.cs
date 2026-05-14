using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, string title)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,

                Flows = new OpenApiOAuthFlows
                {
                    Password = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri("/connect/token", UriKind.Relative),

                        Scopes = new Dictionary<string, string>
                        {
                            ["openid"] = "OpenId scope",
                            ["profile"] = "Profile scope",
                            ["email"] = "Email scope",
                            ["offline_access"] = "Refresh token scope"
                        }
                    }
                }
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new[]
                    {
                        "openid",
                        "profile",
                        "email",
                        "offline_access",
                        "identity",
                        "catalog",
                        "basket",
                        "ordering"
                    }
                }
            });
        });

        return services;
    }
}