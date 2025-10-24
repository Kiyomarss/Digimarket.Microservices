using System.Reflection;
using Basket.Api.StartupExtensions;
using Basket.Core.Services.CheckoutBasket;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Extensions;
using Order.Grpc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();

builder.Host.UseSerilog();

builder.Services.AddConfiguredMediatR(typeof(CreateOrderHandler));

builder.Services.ConfigureServices(builder.Configuration);

//Grpc Services
builder.Services.AddGrpcClient<OrderProtoService.OrderProtoServiceClient>(options => { options.Address = new Uri(builder.Configuration["GrpcSettings:OrderUrl"]!); })
       .ConfigurePrimaryHttpMessageHandler(() =>
       {
           var handler = new HttpClientHandler
           {
               ServerCertificateCustomValidationCallback =
                   HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
           };

           return handler;
       });

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