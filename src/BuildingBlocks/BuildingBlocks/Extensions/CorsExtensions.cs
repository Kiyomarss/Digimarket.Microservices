using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Extensions;

public static class CorsExtensions
{
    private const string PolicyName = "AllowGateway";

    public static string GatewayCorsPolicyName => PolicyName;

    public static IServiceCollection AddGatewayCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }
}