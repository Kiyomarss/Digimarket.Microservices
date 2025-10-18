using System.Diagnostics;
using Basket.Api.StartupExtensions;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Extensions;
using MassTransit.Metadata;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

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

//builder.Services.AddHostedService<RecreateDatabaseHostedService<BasketDbContext>>();

builder.Services.AddOpenTelemetryWithJaeger("Basket API");

//Cross-Cutting Services
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation("Basket API");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();