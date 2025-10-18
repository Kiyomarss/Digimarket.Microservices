using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace BuildingBlocks.Extensions;

public static class SerilogExtensions
{
    public static IHostBuilder UseConfiguredSerilog(this IHostBuilder hostBuilder)
    {
        Log.Logger = new LoggerConfiguration()
                     .MinimumLevel.Information()
                     .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
                     .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                     .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                     .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                     .Enrich.FromLogContext()
                     .WriteTo.Console()
                     .CreateLogger();

        return hostBuilder.UseSerilog();
    }
}