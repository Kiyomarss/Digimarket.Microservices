using BuildingBlocks.Behaviors;
using BuildingBlocks.Extensions;
using BuildingBlocks.Services;
using MassTransit;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Api.Grpc;
using Ordering.Api.StartupExtensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
});
builder.Host.UseConfiguredSerilog();

builder.Host.UseSerilog();

builder.Services.ConfigureServices(builder.Configuration);

//builder.Services.AddHostedService<RecreateDatabaseHostedService<OrderingDbContext>>();

builder.Services.AddOpenTelemetryWithJaeger("Ordering API");
builder.Services.AddGlobalExceptionHandler();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation("Ordering API");

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddMassTransit(x =>
{
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

public partial class Program { }