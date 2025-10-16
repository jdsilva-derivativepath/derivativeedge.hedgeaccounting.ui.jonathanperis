using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DerivativeEDGE.HedgeAccounting.UI.ServiceCollectionExtensions;

public class OtelConfigOptions(IConfiguration configuration)
{
    public Uri CollectorEndpoint { get; } = new Uri(configuration.GetValue<string>("OTEL_ENDPOINT") ?? string.Empty);
    public string ServiceName { get; } = configuration.GetValue<string>("OTEL_SERVICE_NAME") ?? string.Empty;
    public string ServiceVersion { get; } = configuration.GetValue<string>("CONTAINER_TAG") ?? string.Empty;
}

public static class OtelConfig
{
    public static WebApplicationBuilder AddOtel(this WebApplicationBuilder builder)
    {
        var otelConfig = new OtelConfigOptions(builder.Configuration);

        var appResourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName: otelConfig.ServiceName,
                        serviceVersion: otelConfig.ServiceVersion);

        // Create a service to expose ActivitySource, and Metric Instruments
        // for manual instrumentation
        // builder.Services.AddSingleton<Instrumentation>();

        builder.Logging.AddOpenTelemetry(options =>
        {
            options
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(otelConfig.ServiceName))
                .AddOtlpExporter();
        });

        builder.Services.AddOpenTelemetry()
        .WithLogging(loggingProvider =>
             loggingProvider.AddOtlpExporter(o => {
                 o.Endpoint = otelConfig.CollectorEndpoint; // Note: 4317 for gRPC, 4318 for HTTP
                 o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc; // Explicitly set the protocol

                 // Change to Simple processor to see errors immediately
                 o.ExportProcessorType = OpenTelemetry.ExportProcessorType.Batch;
             })
             )

        .WithMetrics(metricsProvider =>
            metricsProvider
                .AddMeter(otelConfig.ServiceName)
                .AddOtlpExporter(o => {
                    o.Endpoint = otelConfig.CollectorEndpoint; // Note: 4317 for gRPC, 4318 for HTTP
                    o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc; // Explicitly set the protocol

                    // Change to Simple processor to see errors immediately
                    o.ExportProcessorType = OpenTelemetry.ExportProcessorType.Batch;
                })
                .SetResourceBuilder(appResourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())

        .WithTracing(tracerProviderBuilder =>
            tracerProviderBuilder
                .AddSource(otelConfig.ServiceName)
                .ConfigureResource(resource => resource
                    .AddService(otelConfig.ServiceName))
                .AddAspNetCoreInstrumentation()
                .AddAWSInstrumentation()
                .SetErrorStatusOnException()
                .AddOtlpExporter(o => {
                    o.Endpoint = otelConfig.CollectorEndpoint; // Note: 4317 for gRPC, 4318 for HTTP
                    o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc; // Explicitly set the protocol

                    // Change to Simple processor to see errors immediately
                    o.ExportProcessorType = OpenTelemetry.ExportProcessorType.Batch;
                })
                //.AddConsoleExporter()
                .AddProcessor(new ErrorHandlingProcessor())
                );

        return builder;
    }

    // Custom processor to handle errors
    public class ErrorHandlingProcessor : BaseProcessor<Activity>
    {
        public override void OnEnd(Activity activity)
        {
            try
            {
                base.OnEnd(activity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting telemetry: {ex}");
                throw;
            }
        }
    }
}