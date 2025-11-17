using Ordering.Worker.Extensions;
using Serilog;

var builder = Host.CreateDefaultBuilder(args)
                  .ConfigureAppConfiguration((hostingContext, config) =>
                  {
                      config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables();
                  })
                  .ConfigureServices((hostContext, services) =>
                  {
                      services.AddOrderingServices(hostContext.Configuration);
                  })
                  .UseSerilog()
                  .Build();

await builder.RunAsync();