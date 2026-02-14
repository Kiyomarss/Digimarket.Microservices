using BuildingBlocks.Extensions;
using BuildingBlocks.Services;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Api.Consumers;
using Ordering.Api.Grpc;
using Ordering.Api.StartupExtensions;
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
builder.Services.AddSwaggerDocumentation("Ordering API");

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddJwtAuthentication(builder.Configuration);

const string serviceName = "ordering.api";

builder.Services.AddConfiguredOpenTelemetry(
                                            serviceName: serviceName,
                                            configuration: builder.Configuration);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(OrderPaidConsumer).Assembly);
    x.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(1);
        o.UsePostgres();
        o.UseBusOutbox();
    });

    // تنها Transport: RabbitMQ
    x.UsingRabbitMq((context, cfg) =>
    {
        // حتماً آدرس و احراز هویت را متناسب با محیط شودد:
        cfg.Host("rabbitmq://localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.UseMessageRetry(r => r.Interval(2, TimeSpan.FromSeconds(1)));
        // ساخت خودکار Queue/Exchange بر اساس Convention
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddSwaggerGen();

builder.Services.AddGatewayCors();

var app = builder.Build();

app.UseCors(CorsExtensions.GatewayCorsPolicyName);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGrpcService<OrderGrpcService>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

namespace Ordering.Api
{
    public partial class Program { }
}