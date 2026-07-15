using Microsoft.Extensions.Logging;
using ThrottleWatch.Application.DTOs.Alerts;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Exceptions;
using ThrottleWatch.Domain.Interfaces;

namespace ThrottleWatch.Application.Services;

public sealed class AlertService : IAlertService
{
    private readonly IAlertRepository _repository;
    private readonly ILogger<AlertService> _logger;

    public AlertService(IAlertRepository repository, ILogger<AlertService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AlertRuleDto>> GetRulesAsync(CancellationToken ct)
    {
        var rules = await _repository.GetAllAsync(ct);
        return rules.Select(ToDto).ToList();
    }

    public async Task<AlertRuleDto> GetRuleByIdAsync(Guid id, CancellationToken ct)
    {
        var rule = await _repository.GetByIdAsync(id, ct)
            ?? throw new AlertRuleNotFoundException(id);

        return ToDto(rule);
    }

    public async Task<AlertRuleDto> CreateRuleAsync(CreateAlertRuleDto dto, CancellationToken ct)
    {
        var rule = AlertRule.Create(
            dto.Name,
            dto.Condition,
            dto.Threshold,
            dto.Severity,
            dto.CooldownMinutes,
            dto.Description);

        await _repository.AddAsync(rule, ct);

        _logger.LogInformation("AlertRule created: {Name} ({Id})", rule.Name, rule.Id);

        return ToDto(rule);
    }

    public async Task<AlertRuleDto> UpdateRuleAsync(Guid id, UpdateAlertRuleDto dto, CancellationToken ct)
    {
        var rule = await _repository.GetByIdAsync(id, ct)
            ?? throw new AlertRuleNotFoundException(id);

        rule.Update(dto.Name, dto.Condition, dto.Threshold, dto.Severity, dto.CooldownMinutes, dto.Description);

        if (dto.IsEnabled) rule.Enable();
        else rule.Disable();

        await _repository.UpdateAsync(rule, ct);

        return ToDto(rule);
    }

    public async Task DeleteRuleAsync(Guid id, CancellationToken ct)
    {
        var rule = await _repository.GetByIdAsync(id, ct)
            ?? throw new AlertRuleNotFoundException(id);

        await _repository.DeleteAsync(rule.Id, ct);

        _logger.LogInformation("AlertRule deleted: {Id}", id);
    }

    public async Task<IReadOnlyList<AlertEventDto>> GetRecentEventsAsync(int count, CancellationToken ct)
    {
        var events = await _repository.GetRecentEventsAsync(count, ct);
        return events.Select(ToEventDto).ToList();
    }

    public async Task AcknowledgeEventAsync(Guid eventId, CancellationToken ct)
    {
        var alertEvent = await _repository.GetEventByIdAsync(eventId, ct)
            ?? throw new AlertEventNotFoundException(eventId);

        alertEvent.Acknowledge();
        await _repository.UpdateEventAsync(alertEvent, ct);
    }

    private static AlertRuleDto ToDto(AlertRule rule) => new(
        rule.Id,
        rule.Name,
        rule.Description,
        rule.Condition,
        rule.Threshold,
        rule.Severity,
        rule.IsEnabled,
        rule.CooldownMinutes,
        rule.LastTriggeredAt,
        rule.CreatedAt);

    private static AlertEventDto ToEventDto(AlertEvent e) => new(
        e.Id,
        e.AlertRuleId,
        e.RuleName,
        e.Message,
        e.Severity,
        e.TriggeredAt,
        e.IsAcknowledged);
}
