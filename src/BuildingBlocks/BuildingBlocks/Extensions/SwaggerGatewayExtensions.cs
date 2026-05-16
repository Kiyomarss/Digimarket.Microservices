using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.Extensions;

public static class SwaggerGatewayExtensions
{
    public static IApplicationBuilder UseGatewaySwagger(
        this IApplicationBuilder app,
        string serviceName,
        string httpUrl,
        string httpsUrl)
    {
        app.UseSwagger(options =>
        {
            options.PreSerializeFilters.Add((swagger, httpReq) =>
            {
                swagger.Servers = new List<OpenApiServer>
                {
                    new OpenApiServer
                    {
                        Url = $"{httpUrl}/{serviceName}"
                    },

                    new OpenApiServer
                    {
                        Url = $"{httpsUrl}/{serviceName}"
                    }
                };
            });
        });

        app.UseSwaggerUI();

        return app;
    }
}