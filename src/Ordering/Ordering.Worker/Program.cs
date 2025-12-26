using Ordering.Worker.Extensions;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// ---------- Configuration ----------
builder.Configuration
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .AddEnvironmentVariables();

// ---------- Services ----------
builder.Services.AddOrderingServices(builder.Configuration, builder.Environment);

// ---------- Build & Run ----------
var host = builder.Build();
await host.RunAsync();