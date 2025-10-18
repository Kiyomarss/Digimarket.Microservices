using System.Diagnostics;
using MassTransit.Metadata;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryWithJaeger(this IServiceCollection services, string serviceName)
    {
        services.AddOpenTelemetry().WithTracing(x =>
        {
            x.SetResourceBuilder(ResourceBuilder.CreateDefault()
                                                .AddService(serviceName)
                                                .AddTelemetrySdk()
                                                .AddEnvironmentVariableDetector())
             .AddSource("MassTransit")
             .AddAspNetCoreInstrumentation()
             .AddJaegerExporter(o =>
             {
                 o.AgentHost = HostMetadataCache.IsRunningInContainer ? "jaeger" : "localhost";
                 o.AgentPort = 6831;
                 o.MaxPayloadSizeInBytes = 4096;
                 o.ExportProcessorType = ExportProcessorType.Batch;
                 o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                 {
                     MaxQueueSize = 2048,
                     ScheduledDelayMilliseconds = 5000,
                     ExporterTimeoutMilliseconds = 30000,
                     MaxExportBatchSize = 512,
                 };
             });
        });

        return services;
    }
}