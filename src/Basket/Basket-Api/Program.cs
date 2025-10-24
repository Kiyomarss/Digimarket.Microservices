using Basket.Api.StartupExtensions;
using BuildingBlocks.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();

builder.Host.UseSerilog();

builder.Services.ConfigureServices(builder.Configuration);

//builder.Services.AddHostedService<RecreateDatabaseHostedService<BasketDbContext>>();

builder.Services.AddOpenTelemetryWithJaeger("Basket API");
builder.Services.AddGlobalExceptionHandler();
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