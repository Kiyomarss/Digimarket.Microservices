using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Extensions;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddConfiguredOpenTelemetry(
        this IServiceCollection services,
        string serviceName,
        IConfiguration? configuration = null)
    {
        services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                                       .AddService(
                                                   serviceName: serviceName,
                                                   serviceVersion: "1.0.0"))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.RecordException = true;
                            // می‌توان گزینه‌های دیگه مثل Enrich رو هم اضافه کرد
                        })
                        .AddHttpClientInstrumentation();

                    tracing.AddNpgsql();
                    
                    // اگر در همه سرویس‌ها ActivitySource سفارشی دارید (مثلاً برای manual tracing)
                    tracing.AddSource(serviceName);

                    // OTLP Exporter
                    tracing.AddOtlpExporter(options =>
                    {
                        var endpoint = configuration?.GetValue<string>("OTLP:Endpoint") 
                                       ?? Environment.GetEnvironmentVariable("OTLP_ENDPOINT")
                                       ?? "http://localhost:4317";

                        options.Endpoint = new Uri(endpoint);
                    });
                });

        return services;
    }
}