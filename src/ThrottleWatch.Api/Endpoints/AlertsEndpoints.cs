using FluentValidation;
using ThrottleWatch.Application.DTOs.Alerts;
using ThrottleWatch.Application.Services;

namespace ThrottleWatch.Api.Endpoints;

public static class AlertsEndpoints
{
    public static IEndpointRouteBuilder MapAlertsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/alerts")
            .WithTags("Alerts");

        group.MapGet("/rules", MapGetRules)
            .WithName("GetAlertRules");

        group.MapGet("/rules/{id:guid}", MapGetRuleById)
            .WithName("GetAlertRuleById");

        group.MapPost("/rules", MapCreateRule)
            .WithName("CreateAlertRule");

        group.MapPut("/rules/{id:guid}", MapUpdateRule)
            .WithName("UpdateAlertRule");

        group.MapDelete("/rules/{id:guid}", MapDeleteRule)
            .WithName("DeleteAlertRule");

        group.MapGet("/events", MapGetRecentEvents)
            .WithName("GetRecentAlertEvents");

        group.MapPost("/events/{eventId:guid}/acknowledge", MapAcknowledgeEvent)
            .WithName("AcknowledgeAlertEvent");

        return app;
    }

    private static async Task<IResult> MapGetRules(
        IAlertService alertService,
        CancellationToken ct)
    {
        var rules = await alertService.GetRulesAsync(ct);
        return Results.Ok(rules);
    }

    private static async Task<IResult> MapGetRuleById(
        Guid id,
        IAlertService alertService,
        CancellationToken ct)
    {
        var rule = await alertService.GetRuleByIdAsync(id, ct);
        return Results.Ok(rule);
    }

    private static async Task<IResult> MapCreateRule(
        CreateAlertRuleDto dto,
        IValidator<CreateAlertRuleDto> validator,
        IAlertService alertService,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(dto, ct);
        var created = await alertService.CreateRuleAsync(dto, ct);
        return Results.Created($"/api/alerts/rules/{created.Id}", created);
    }

    private static async Task<IResult> MapUpdateRule(
        Guid id,
        UpdateAlertRuleDto dto,
        IValidator<UpdateAlertRuleDto> validator,
        IAlertService alertService,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(dto, ct);
        var updated = await alertService.UpdateRuleAsync(id, dto, ct);
        return Results.Ok(updated);
    }

    private static async Task<IResult> MapDeleteRule(
        Guid id,
        IAlertService alertService,
        CancellationToken ct)
    {
        await alertService.DeleteRuleAsync(id, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> MapGetRecentEvents(
        int? count,
        IAlertService alertService,
        CancellationToken ct)
    {
        var take = count is > 0 ? count.Value : 20;
        var events = await alertService.GetRecentEventsAsync(take, ct);
        return Results.Ok(events);
    }

    private static async Task<IResult> MapAcknowledgeEvent(
        Guid eventId,
        IAlertService alertService,
        CancellationToken ct)
    {
        await alertService.AcknowledgeEventAsync(eventId, ct);
        return Results.NoContent();
    }
}
