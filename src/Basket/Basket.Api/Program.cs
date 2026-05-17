using Basket_Application.Basket.Consumers;
using Basket.Api.StartupExtensions;
using Basket.Infrastructure.Data.DbContext;
using BuildingBlocks.Extensions;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Services;
using Microsoft.OpenApi.Models;
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
builder.Services.AddSwaggerDocumentation();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddConfiguredMassTransit<BasketDbContext>(builder.Configuration, typeof(RemoveBasketConsumer).Assembly);

builder.Services.AddGatewayCors();

var app = builder.Build();

app.UseCors(CorsExtensions.GatewayCorsPolicyName);

if (app.Environment.IsDevelopment())
{
    app.UseGatewaySwagger(
                          "basket",
                          "http://localhost:1000",
                          "https://localhost:1001");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();