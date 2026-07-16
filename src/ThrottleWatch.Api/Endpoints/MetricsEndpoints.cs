using ThrottleWatch.Application.DTOs.Metrics;
using ThrottleWatch.Application.Services;

namespace ThrottleWatch.Api.Endpoints;

public static class MetricsEndpoints
{
    public static IEndpointRouteBuilder MapMetricsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/metrics")
            .WithTags("Metrics");

        group.MapPost("/", MapIngestMetrics)
            .WithName("IngestMetrics");

        group.MapGet("/summary", MapGetSummary)
            .WithName("GetMetricsSummary");

        group.MapGet("/top-endpoints", MapGetTopEndpoints)
            .WithName("GetTopEndpoints");

        group.MapGet("/top-clients", MapGetTopClients)
            .WithName("GetTopClients");

        group.MapGet("/timeseries", MapGetTimeSeries)
            .WithName("GetTimeSeries");

        return app;
    }

    private static async Task<IResult> MapIngestMetrics(
        IReadOnlyList<IngestMetricDto> metrics,
        IMetricsService metricsService,
        CancellationToken ct)
    {
        if (metrics.Count == 0)
            return Results.BadRequest("Metrics batch cannot be empty.");

        await metricsService.EnqueueBatchAsync(metrics, ct);
        return Results.Accepted();
    }

    private static async Task<IResult> MapGetSummary(
        DateTimeOffset? from,
        DateTimeOffset? to,
        IMetricsService metricsService,
        CancellationToken ct)
    {
        var rangeTo = to ?? DateTimeOffset.UtcNow;
        var rangeFrom = from ?? rangeTo.AddHours(-24);

        var summary = await metricsService.GetSummaryAsync(rangeFrom, rangeTo, ct);
        return Results.Ok(summary);
    }

    private static async Task<IResult> MapGetTopEndpoints(
        int? top,
        DateTimeOffset? from,
        IMetricsService metricsService,
        CancellationToken ct)
    {
        var take = top is > 0 ? top.Value : 10;
        var rangeFrom = from ?? DateTimeOffset.UtcNow.AddHours(-24);

        var result = await metricsService.GetTopEndpointsAsync(take, rangeFrom, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> MapGetTopClients(
        int? top,
        DateTimeOffset? from,
        IMetricsService metricsService,
        CancellationToken ct)
    {
        var take = top is > 0 ? top.Value : 10;
        var rangeFrom = from ?? DateTimeOffset.UtcNow.AddHours(-24);

        var result = await metricsService.GetTopClientsAsync(take, rangeFrom, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> MapGetTimeSeries(
        DateTimeOffset? from,
        DateTimeOffset? to,
        IMetricsService metricsService,
        CancellationToken ct)
    {
        var rangeTo = to ?? DateTimeOffset.UtcNow;
        var rangeFrom = from ?? rangeTo.AddHours(-24);

        var result = await metricsService.GetTimeSeriesAsync(rangeFrom, rangeTo, ct);
        return Results.Ok(result);
    }
}
