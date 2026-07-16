using ThrottleWatch.Application.Services;

namespace ThrottleWatch.Api.Endpoints;

public static class InsightsEndpoints
{
    public static IEndpointRouteBuilder MapInsightsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/insights")
            .WithTags("Insights");

        group.MapGet("/", MapGetActiveInsights)
            .WithName("GetActiveInsights");

        group.MapPost("/{id:guid}/dismiss", MapDismissInsight)
            .WithName("DismissInsight");

        return app;
    }

    private static async Task<IResult> MapGetActiveInsights(
        IInsightService insightService,
        CancellationToken ct)
    {
        var insights = await insightService.GetActiveInsightsAsync(ct);
        return Results.Ok(insights);
    }

    private static async Task<IResult> MapDismissInsight(
        Guid id,
        IInsightService insightService,
        CancellationToken ct)
    {
        await insightService.DismissInsightAsync(id, ct);
        return Results.NoContent();
    }
}
