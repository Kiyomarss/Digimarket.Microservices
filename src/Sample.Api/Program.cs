using System.Diagnostics;
using System.Reflection;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using Carter;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.Metadata;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sample.Api;
using Sample.Components;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddScoped<IRegistrationService, RegistrationService>();

builder.Services.AddCarter();

var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddDbContext<RegistrationDbContext>(x =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");

    x.UseNpgsql(connectionString, options =>
    {
        options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
        options.MigrationsHistoryTable($"__{nameof(RegistrationDbContext)}");

        options.EnableRetryOnFailure(5);
        options.MinBatchSize(1);
    });
});

builder.Services.AddHostedService<RecreateDatabaseHostedService<RegistrationDbContext>>();

builder.Services.AddOpenTelemetry().WithTracing(x =>
{
    x.SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("api")
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector())
        .AddSource("MassTransit")
        .AddAspNetCoreInstrumentation()
        .AddJaegerExporter(o =>
        {
            o.AgentHost = HostMetadataCache.IsRunningInContainer ? "jaeger" : "localhost";
            o.AgentPort = 6831;
            o.MaxPayloadSizeInBytes = 4096;
            o.ExportProcessorType = ExportProcessorType.Batch;
            o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
            {
                MaxQueueSize = 2048,
                ScheduledDelayMilliseconds = 5000,
                ExporterTimeoutMilliseconds = 30000,
                MaxExportBatchSize = 512,
            };
        });
});
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<RegistrationDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(1);

        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((_, cfg) =>
    {
        cfg.AutoStart = true;
    });
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    //options.InstanceName = "Basket";
});

//Cross-Cutting Services
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapCarter();

app.Run();