using Basket.Api.StartupExtensions;
using BuildingBlocks.Extensions;
using BuildingBlocks.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();
builder.Host.UseSerilog();

builder.Services.ConfigureServices(builder.Configuration);

const string serviceName = "basket-api";

builder.Services.AddConfiguredOpenTelemetry(
                                            serviceName: serviceName,
                                            configuration: builder.Configuration);
builder.Services.AddGlobalExceptionHandler();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation("Basket API");

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddGatewayCors();

var app = builder.Build();

app.UseCors(CorsExtensions.GatewayCorsPolicyName);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();