using OpenTelemetry.Metrics;
using Serilog;
using ThrottleWatch.Api.Endpoints;
using ThrottleWatch.Api.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddThrottleWatchSerilog();
    builder.Services.AddApiServices(builder.Configuration, builder.Environment);
    builder.Services.AddThrottleWatchOpenApi();

    builder.Services.ConfigureOpenTelemetryMeterProvider(metrics =>
        metrics.AddPrometheusExporter());

    var app = builder.Build();

    await app.ApplyMigrationsIfConfiguredAsync();

    app.UseExceptionHandler();
    app.UseThrottleWatchSerilogRequestLogging();
    app.UseThrottleWatchOpenApi();
    app.UseHttpsRedirection();
    app.UseCors();

    app.MapMetricsEndpoints();
    app.MapAlertsEndpoints();
    app.MapInsightsEndpoints();
    app.MapHealthChecks("/health");
    app.MapPrometheusScrapingEndpoint("/metrics");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ThrottleWatch.Api terminated unexpectedly.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
