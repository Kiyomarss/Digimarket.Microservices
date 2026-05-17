using BuildingBlocks.Extensions;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Services;
using MassTransit;
using Microsoft.OpenApi.Models;
using Ordering_Infrastructure.Data.DbContext;
using Ordering_Infrastructure.Realtime.Hubs;
using Ordering_Infrastructure.Realtime.Services;
using Ordering.Api.Grpc;
using Ordering.Api.StartupExtensions;
using Ordering.Application.Orders.Consumers;
using Ordering.Application.RepositoryContracts.Realtime;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GrpcExceptionInterceptor>();
    options.EnableDetailedErrors = true;
});
builder.Host.UseConfiguredSerilog();

builder.Host.UseSerilog();

builder.Services.ConfigureServices(builder.Configuration);

//builder.Services.AddHostedService<RecreateDatabaseHostedService<OrderingDbContext>>();

builder.Services.AddGlobalExceptionHandler();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddJwtAuthentication(builder.Configuration);

const string serviceName = "ordering.api";

builder.Services.AddConfiguredOpenTelemetry(
                                            serviceName: serviceName,
                                            configuration: builder.Configuration);

builder.Services.AddConfiguredMassTransit<OrderingDbContext>(builder.Configuration, typeof(OrderCanceledConsumer).Assembly);

builder.Services.AddSwaggerGen();

builder.Services.AddGatewayCors();

builder.Services.AddSignalR();

builder.Services.AddScoped<IOrderStatusNotifier, OrderStatusNotifier>();

var app = builder.Build();

app.UseCors(CorsExtensions.GatewayCorsPolicyName);

if (app.Environment.IsDevelopment())
{
    app.UseGatewaySwagger(
                          serviceName: "ordering",
                          httpUrl: "http://localhost:1000",
                          httpsUrl: "https://localhost:1001");
}

app.MapGrpcService<OrderGrpcService>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<OrderHub>("/hubs/orders");

app.Run();

namespace Ordering.Api
{
    public partial class Program { }
}