using System.Diagnostics;
using System.Reflection;
using Basket.Api.StartupExtensions;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Extensions;
using Carter;
using MassTransit;
using MassTransit.Metadata;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Api;
using Ordering.Components;
using Serilog;
using Serilog.Events;
using Product.Grpc;

var builder = WebApplication.CreateBuilder(args);

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

//Grpc Services
builder.Services.AddGrpcClient<ProductService.ProductServiceClient>(options => { options.Address = new Uri(builder.Configuration["GrpcSettings:CatalogUrl"]!); })
       .ConfigurePrimaryHttpMessageHandler(() =>
       {
           var handler = new HttpClientHandler
           {
               ServerCertificateCustomValidationCallback =
                   HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
           };

           return handler;
       });

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();