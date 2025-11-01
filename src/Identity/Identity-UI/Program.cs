using System.Security.Cryptography;
using System.Text;
using Identity_Infrastructure.DbContext;
using Identity_UI.Filters;
using Identity_UI.Middleware;
using Identity_UI.StartupExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------------------- Serilog ----------------------
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

// ---------------------- Configure Services ----------------------
builder.Services.ConfigureServices(builder.Configuration);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// ---------------------- RSA: خواندن کلید خصوصی PEM ----------------------
var privateKeyPath = builder.Configuration["Jwt:PrivateKeyPath"];

if (string.IsNullOrWhiteSpace(privateKeyPath) || !File.Exists(privateKeyPath))
    throw new FileNotFoundException($"Private key not found at: {privateKeyPath}");

var privateKeyPem = await File.ReadAllTextAsync(privateKeyPath);

// ساخت RSA برای امضا (Signing)
var rsaForSigning = RSA.Create();
rsaForSigning.ImportFromPem(privateKeyPem); // ImportFromPem نوع PEM را خودکار تشخیص می‌دهد

// ساخت RSA برای اعتبارسنجی (فقط public)
var rsaForValidation = RSA.Create();
rsaForValidation.ImportFromPem(rsaForSigning.ExportSubjectPublicKeyInfoPem());

// ساخت SecurityKey برای JWT
var rsaValidationKey = new RsaSecurityKey(rsaForValidation)
{
    KeyId = Guid.NewGuid().ToString() // kid اختیاری است، اما برای JWKS خوب است
};

// ---------------------- Authentication / Authorization ----------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // در محیط production باید true شود
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = rsaValidationKey
    };
});

builder.Services.AddAuthorization();

// ---------------------- Swagger ----------------------
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Identity API",
        Version = "v1"
    });

    options.MapType<Stream>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

    options.OperationFilter<FileUploadOperationFilter>();

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the JWT token: Bearer {your_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

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
            new List<string>()
        }
    });
});

// ---------------------- DbContext ----------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------- Build App ----------------------
var app = builder.Build();

app.UseExceptionHandlingMiddleware();
app.UseSerilogRequestLogging();
app.UseHttpLogging();

// ---------------------- Development Only ----------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseCors("AllowFrontends");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ---------------------- JWKS Endpoint ----------------------
// این endpoint به سایر میکروسرویس‌ها اجازه می‌دهد کلید عمومی را دریافت کنند
app.MapGet("/.well-known/jwks.json", () =>
{
    var rsaParameters = rsaForSigning.ExportParameters(false);
    var e = Base64UrlEncoder.Encode(rsaParameters.Exponent);
    var n = Base64UrlEncoder.Encode(rsaParameters.Modulus);

    var jwk = new
    {
        keys = new[]
        {
            new {
                kty = "RSA",
                use = "sig",
                alg = "RS256",
                kid = rsaValidationKey.KeyId,
                e,
                n
            }
        }
    };

    return Results.Json(jwk);
})
.WithMetadata(new Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute());

app.Run();