using BuildingBlocks.Behaviors;
using BuildingBlocks.Extensions;
using BuildingBlocks.Services;
using Catalog.Api.Grpc;
using Catalog.Api.StartupExtensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GrpcExceptionInterceptor>();
    options.EnableDetailedErrors = true;
});
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

const string serviceName = "catalog-api";

builder.Services.AddConfiguredOpenTelemetry(
                                            serviceName: serviceName,
                                            configuration: builder.Configuration);
builder.Services.AddGlobalExceptionHandler();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation("Catalog API");

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddGatewayCors();

var app = builder.Build();

app.UseCors(CorsExtensions.GatewayCorsPolicyName);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGrpcService<ProductGrpcService>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();