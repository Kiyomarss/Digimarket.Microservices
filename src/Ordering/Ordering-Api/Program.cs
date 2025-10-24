using BuildingBlocks.Behaviors;
using BuildingBlocks.Extensions;
using MassTransit;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Api.StartupExtensions;
using Ordering.Core.Services;
using Serilog;
using Product.Grpc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Host.UseConfiguredSerilog();

builder.Host.UseSerilog();

var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.ConfigureServices(builder.Configuration);

//builder.Services.AddHostedService<RecreateDatabaseHostedService<OrderingDbContext>>();

builder.Services.AddOpenTelemetryWithJaeger("Ordering API");
builder.Services.AddGlobalExceptionHandler();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation("Ordering API");

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGrpcService<OrderGrpcService>();
app.MapControllers();

app.Run();