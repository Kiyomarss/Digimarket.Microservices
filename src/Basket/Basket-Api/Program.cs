using Basket.Api.StartupExtensions;
using BuildingBlocks.Extensions;
using BuildingBlocks.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();
builder.Host.UseSerilog();

builder.Services.ConfigureServices(builder.Configuration);
builder.Services.AddOpenTelemetryWithJaeger("Basket API");
builder.Services.AddGlobalExceptionHandler();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation("Basket API");

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ✅ افزودن احراز هویت از طریق اکستنشن
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();