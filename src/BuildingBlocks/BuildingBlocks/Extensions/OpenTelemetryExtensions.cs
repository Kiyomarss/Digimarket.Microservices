using System.Diagnostics;
using MassTransit.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
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
                            // می‌تونید گزینه‌های دیگه مثل Enrich رو هم اضافه کنید
                        })
                        .AddHttpClientInstrumentation();

                    // اگر در همه سرویس‌ها ActivitySource سفارشی دارید (مثلاً برای manual tracing)
                    tracing.AddSource(serviceName);

                    // OTLP Exporter
                    tracing.AddOtlpExporter(options =>
                    {
                        // اولویت: از configuration بخونه، اگر نبود از محیطی یا default
                        var endpoint = configuration?.GetValue<string>("OTLP:Endpoint") 
                                       ?? Environment.GetEnvironmentVariable("OTLP_ENDPOINT")
                                       ?? "http://localhost:4317";

                        options.Endpoint = new Uri(endpoint);
                    });
                });

        return services;
    }
}