using System.Diagnostics;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Extensions;
using MassTransit.Metadata;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Catalog.Api.StartupExtensions;
using Serilog;

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

//builder.Services.AddHostedService<RecreateDatabaseHostedService<CatalogDbContext>>();

builder.Services.AddOpenTelemetryWithJaeger("Basket API");

//Cross-Cutting Services
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation("Catalog API");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();