using BuildingBlocks.Extensions;
using BuildingBlocks.Messaging.Extensions;
using Identity.Api.StartupExtensions;
using Identity.Application.Consumers;
using Identity.Infrastructure.Data.DbContext;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();
builder.Host.UseSerilog();

builder.Services.ConfigureServices(builder.Configuration);

builder.Services.AddGlobalExceptionHandler();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation();

builder.Services.AddHttpContextAccessor();

builder.Services.AddConfiguredMassTransit<IdentityDbContext>(
                                                             builder.Configuration,
                                                             typeof(IdentityConsumer).Assembly);


// ----------------------------
// OPENIDDICT
// ----------------------------

builder.Services.AddOpenIddict()
       .AddCore(options =>
       {
           options.UseEntityFrameworkCore()
                  .UseDbContext<IdentityDbContext>();
       })
       .AddServer(options =>
       {
           options.SetIssuer(new Uri("https://localhost:2001"));

           // Endpoints
           options.SetTokenEndpointUris("/connect/token");
           options.SetAuthorizationEndpointUris("/connect/authorize");

           // Flows
           options.AllowPasswordFlow();
           options.AllowRefreshTokenFlow();

           // Scopes
           options.RegisterScopes(
                                  "identity",
                                  "basket",
                                  "catalog",
                                  "ordering");

           // Dev only
           options.AcceptAnonymousClients();

           // JWT
           options.DisableAccessTokenEncryption();

           // Certificates
           options.AddDevelopmentEncryptionCertificate();
           options.AddDevelopmentSigningCertificate();

           // ASP.NET Core
           options.UseAspNetCore()
                  .EnableTokenEndpointPassthrough()
                  .EnableAuthorizationEndpointPassthrough()
                  .EnableTokenEndpointPassthrough()
                  .EnableStatusCodePagesIntegration();

       })
       .AddValidation(options =>
       {
           options.UseLocalServer();

           options.UseAspNetCore();
       });


// ----------------------------
// AUTHENTICATION
// ----------------------------

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});


// ----------------------------
// CORS
// ----------------------------

builder.Services.AddGatewayCors();

var app = builder.Build();

app.UseHttpsRedirection();

// ----------------------------
// PIPELINE
// ----------------------------

app.UseRouting();

app.UseCors(CorsExtensions.GatewayCorsPolicyName);

app.UseForwardedHeaders();

app.UseAuthentication();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.PreSerializeFilters.Add((swagger, httpReq) =>
        {
            swagger.Servers = new List<OpenApiServer>
            {
                new OpenApiServer
                {
                    Url = "http://localhost:1000/identity"
                },

                new OpenApiServer
                {
                    Url = "https://localhost:1001/identity"
                }
            };
        });
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API");

        options.OAuthClientId("swagger");
    });
}

app.MapControllers();

app.Run();