using ApiGateway;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//
// Reverse Proxy (YARP)
//
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:2001";
        options.RequireHttpsMetadata = false;
    });

builder.Services.AddAuthorization();

//
// Swagger
//
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Digimarket Gateway",
        Version = "v1"
    });

    //
    // OAuth2 Password Flow
    //
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,

        Flows = new OpenApiOAuthFlows
        {
            Password = new OpenApiOAuthFlow
            {
                //
                // IMPORTANT:
                // Token endpoint MUST go through Gateway
                //
                TokenUrl = new Uri("https://localhost:1001/identity/connect/token"),
                
                Scopes = new Dictionary<string, string>
                {
                    { "identity", "Identity API" },
                    { "catalog", "Catalog API" },
                    { "basket", "Basket API" },
                    { "ordering", "Ordering API" }
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
                    Id = "oauth2"
                }
            },
            new[]
            {
                "identity",
                "catalog",
                "basket",
                "ordering"
            }

        }
    });
});

var swaggerServices =
    builder.Configuration
        .GetSection("SwaggerServices")
        .Get<List<SwaggerService>>()
    ?? new List<SwaggerService>();

var app = builder.Build();

app.UseHttpsRedirection();

//
// Swagger
//
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    //
    // Aggregate Microservices Swagger
    //
    foreach (var svc in swaggerServices)
    {
        options.SwaggerEndpoint(
            svc.SwaggerEndpoint,
            svc.Name);
    }

    //
    // OAuth Configuration
    //
    options.OAuthClientId("swagger");

    //
    // DO NOT USE PKCE WITH PASSWORD FLOW
    //
    // options.OAuthUsePkce();

    //
    // Optional
    //
    options.OAuthAppName("Digimarket Swagger Gateway");
});

//
// Authentication / Authorization
// (later when JWT validation added)
//
app.UseAuthentication();
app.UseAuthorization();

//
// Reverse Proxy
//
app.MapReverseProxy();

app.Run();

namespace ApiGateway
{
    public class SwaggerService
    {
        public string Name { get; set; } = default!;

        public string SwaggerEndpoint { get; set; } = default!;
    }
}