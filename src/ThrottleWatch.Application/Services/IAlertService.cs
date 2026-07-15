using ThrottleWatch.Application.DTOs.Alerts;

namespace ThrottleWatch.Application.Services;

public interface IAlertService
{
    Task<IReadOnlyList<AlertRuleDto>> GetRulesAsync(CancellationToken ct);
    Task<AlertRuleDto> GetRuleByIdAsync(Guid id, CancellationToken ct);
    Task<AlertRuleDto> CreateRuleAsync(CreateAlertRuleDto dto, CancellationToken ct);
    Task<AlertRuleDto> UpdateRuleAsync(Guid id, UpdateAlertRuleDto dto, CancellationToken ct);
    Task DeleteRuleAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<AlertEventDto>> GetRecentEventsAsync(int count, CancellationToken ct);
    Task AcknowledgeEventAsync(Guid eventId, CancellationToken ct);
}
