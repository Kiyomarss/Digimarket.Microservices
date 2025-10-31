using ContactsManager.Core.Domain.IdentityEntities;
using Hesabdari_Core.ServiceContracts;
using Hesabdari_Core.ServiceContracts.Base;
using Hesabdari_Core.ServiceContracts.Storage;
using Hesabdari_Core.Services;
using Hesabdari_Core.Services.Base;
using Hesabdari_Core.Services.Storage;
using Hesabdari_Infrastructure.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using RepositoryContracts;

namespace Hesabdari_UI
{
 public static class ConfigureServicesExtension
 {
  public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
  {
   services.AddControllers();
   
   services.AddHttpContextAccessor();
   
   services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
   
   services.AddScoped<IUnitOfWork, UnitOfWork>();
   
   services.AddScoped<IAuthService, AuthService>();
   
   services.AddScoped<ITokenService, TokenService>();
   
   services.AddScoped<IRoleClaimService, RoleClaimService>();
   services.AddScoped<IUserClaimService, UserClaimService>();
   
   services.AddScoped<IIdentityService, IdentityService>();
   services.AddScoped<IUserLoginService, UserLoginService>();
   services.AddScoped<IUserTokenService, UserTokenService>();
   
   services.AddScoped<IImageStorageService, ImageStorageService>();
   services.AddScoped<IVideoStorageService, VideoStorageService>();

   services.AddDbContext<ApplicationDbContext>(options =>
   {
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
   });
   
   services.AddEndpointsApiExplorer();

   services.AddApiVersioning(options =>
   {
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
   });

   services.AddResponseCompression(options =>
   {
    options.EnableForHttps = true;
   });
   
   services.AddCors(options =>
   {
    options.AddPolicy("AllowFrontends", builder =>
    {
     builder.WithOrigins(
                         "http://setareganpak.com",
                         "https://setareganpak.com",
                         "http://www.setareganpak.com",
                         "https://www.setareganpak.com",
                         "http://localhost:5000",
                         "http://localhost:3090",
                         "http://localhost:5125"
                        )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
   });

   
   services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
     options.SerializerSettings.ContractResolver = new DefaultContractResolver
     {
      NamingStrategy = new CamelCaseNamingStrategy()
     };
    });
   
   services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
     options.Password.RequiredLength = 5;
     options.Password.RequireNonAlphanumeric = false;
     options.Password.RequireUppercase = false;
     options.Password.RequireLowercase = true;
     options.Password.RequireDigit = false;
     options.Password.RequiredUniqueChars = 3; //Eg: AB12AB (unique characters are A,B,1,2)
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()

    .AddDefaultTokenProviders()

    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()

    .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();
   
   services.AddHttpLogging(options =>
   {
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
   });

   return services;
  }
 }
}
