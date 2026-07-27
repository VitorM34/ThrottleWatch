using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Infrastructure.Telemetry;

namespace ThrottleWatch.Infrastructure.Extensions;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddThrottleWatchTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton<ThrottleWatchMetrics>();
        services.AddSingleton<IOperationalMetrics>(sp => sp.GetRequiredService<ThrottleWatchMetrics>());

        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: configuration["OpenTelemetry:ServiceName"] ?? "ThrottleWatch.Api",
                    serviceVersion: typeof(ObservabilityExtensions).Assembly.GetName().Version?.ToString() ?? "1.0.0"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                if (environment.IsDevelopment())
                    tracing.AddConsoleExporter();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                        options.Endpoint = new Uri(otlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(ThrottleWatchMetrics.MeterName);

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options =>
                        options.Endpoint = new Uri(otlpEndpoint));
                }
            });

        return services;
    }
}
