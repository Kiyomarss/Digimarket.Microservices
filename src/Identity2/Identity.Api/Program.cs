using BuildingBlocks.Extensions;
using BuildingBlocks.Messaging.Extensions;
using Identity.Api.StartupExtensions;
using Identity.Application.Consumers;
using Identity.Infrastructure.Data.DbContext;
using OpenIddict.Validation.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();
builder.Host.UseSerilog();

builder.Services.ConfigureServices(builder.Configuration);

builder.Services.AddGlobalExceptionHandler();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation("Identity API");

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
                  .EnableAuthorizationEndpointPassthrough();
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

    options.DefaultScheme =
        OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});


// ----------------------------
// CORS
// ----------------------------

builder.Services.AddGatewayCors();

var app = builder.Build();


// ----------------------------
// PIPELINE
// ----------------------------

app.UseCors(CorsExtensions.GatewayCorsPolicyName);

app.UseAuthentication();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();