using System.Net.Http.Json;
using ThrottleWatch.Dashboard.DTOs;
using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.Services;

public sealed class AlertsService : IAlertsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AlertsService> _logger;

    public AlertsService(HttpClient httpClient, ILogger<AlertsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AlertRuleInfo>> GetRulesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<AlertRuleApiDto>>(
                "api/alerts/rules", cancellationToken);
            return dtos?.Select(d => d.ToModel()).ToList() ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch alert rules");
            return [];
        }
    }

    public async Task<AlertRuleInfo?> CreateRuleAsync(
        AlertRuleFormModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateAlertRuleRequest(
                model.Name.Trim(),
                model.Condition.Trim(),
                model.Threshold,
                model.Severity,
                model.CooldownMinutes,
                string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim());

            using var response = await _httpClient.PostAsJsonAsync(
                "api/alerts/rules", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Create alert rule failed with HTTP {StatusCode}",
                    (int)response.StatusCode);
                return null;
            }

            var dto = await response.Content.ReadFromJsonAsync<AlertRuleApiDto>(cancellationToken);
            return dto?.ToModel();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to create alert rule");
            return null;
        }
    }

    public async Task<AlertRuleInfo?> UpdateRuleAsync(
        AlertRuleFormModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.Id is null)
            return null;

        try
        {
            var request = new UpdateAlertRuleRequest(
                model.Name.Trim(),
                model.Condition.Trim(),
                model.Threshold,
                model.Severity,
                model.CooldownMinutes,
                model.IsEnabled,
                string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim());

            using var response = await _httpClient.PutAsJsonAsync(
                $"api/alerts/rules/{model.Id}", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Update alert rule failed with HTTP {StatusCode}",
                    (int)response.StatusCode);
                return null;
            }

            var dto = await response.Content.ReadFromJsonAsync<AlertRuleApiDto>(cancellationToken);
            return dto?.ToModel();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to update alert rule {RuleId}", model.Id);
            return null;
        }
    }

    public async Task<bool> DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.DeleteAsync(
                $"api/alerts/rules/{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to delete alert rule {RuleId}", id);
            return false;
        }
    }

    public async Task<IReadOnlyList<AlertInfo>> GetEventsAsync(
        int count = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<AlertEventApiDto>>(
                $"api/alerts/events?count={count}", cancellationToken);
            return dtos?.Select(d => d.ToModel()).ToList() ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch alert events");
            return [];
        }
    }

    public async Task<bool> AcknowledgeEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsync(
                $"api/alerts/events/{eventId}/acknowledge",
                content: null,
                cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to acknowledge alert event {EventId}", eventId);
            return false;
        }
    }
}
