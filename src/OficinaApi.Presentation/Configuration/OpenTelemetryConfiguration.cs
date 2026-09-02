using System.Diagnostics.Metrics;
using OficinaApi.Infrastructure.Metrics;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OficinaApi.Presentation.Configuration;

public static class OpenTelemetryConfiguration
{
    public static IServiceCollection AddOTelConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["OTEL_SERVICE_NAME"] ?? nameof(OficinaApi);

        services.AddOpenTelemetry()
            .UseOtlpExporter()
            .ConfigureResource(resourceBuilder =>
            {
                resourceBuilder
                    .AddService(serviceName: serviceName,
                                serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown");
            })
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation(options =>
                        {
                            options.Filter = httpContext =>
                                !httpContext.Request.Path.StartsWithSegments("/health");
                            options.RecordException = true;
                        }
                    );
            })
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .AddMeter(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddView(instrument =>
                    {
                        return instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>)
                            ? new Base2ExponentialBucketHistogramConfiguration()
                            : null;
                    })
                    .AddMeter(ApplicationMetrics.ServiceOrderStatusMeanTime.Name);
            });

        return services;
    }

    public static void AddOTelLogging(this ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;
            options.IncludeScopes = true;
        });
    }

}
