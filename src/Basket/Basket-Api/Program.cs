using Basket.Api.StartupExtensions;
using Basket.Core.Services.CheckoutBasket;
using BuildingBlocks.Extensions;
using Order.Grpc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();

builder.Host.UseSerilog();

builder.Services.AddConfiguredMediatR(typeof(CreateOrderHandler));

builder.Services.ConfigureServices(builder.Configuration);

builder.Services.AddGrpcClientWithConfig<OrderProtoService.OrderProtoServiceClient>(builder.Configuration, "GrpcSettings:OrderUrl");

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