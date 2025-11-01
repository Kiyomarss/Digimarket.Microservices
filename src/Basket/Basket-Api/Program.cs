using System.Security.Cryptography;
using Basket.Api.StartupExtensions;
using BuildingBlocks.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();
builder.Host.UseSerilog();

builder.Services.ConfigureServices(builder.Configuration);
builder.Services.AddOpenTelemetryWithJaeger("Basket API");
builder.Services.AddGlobalExceptionHandler();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation("Basket API");

// ---------------------- RSA: خواندن کلید عمومی PEM ----------------------
var publicKeyPath = builder.Configuration["Jwt:PublicKeyPath"];

if (string.IsNullOrWhiteSpace(publicKeyPath) || !File.Exists(publicKeyPath))
    throw new FileNotFoundException($"Public key not found at: {publicKeyPath}");

var publicKeyPem = await File.ReadAllTextAsync(publicKeyPath);

using var rsa = RSA.Create();
rsa.ImportFromPem(publicKeyPem); // ✅ ImportFromPem خودش تشخیص می‌دهد که PEM از چه نوعی است
var rsaKey = new RsaSecurityKey(rsa);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.RequireHttpsMetadata = false;
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidIssuer = builder.Configuration["Jwt:Issuer"],
               ValidateAudience = true,
               ValidAudience = builder.Configuration["Jwt:Audience"],
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               IssuerSigningKey = rsaKey
           };
       });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();