using System.Net.Http.Json;
using ThrottleWatch.Dashboard.DTOs;
using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.Services;

public sealed class InsightsService : IInsightsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InsightsService> _logger;

    public InsightsService(HttpClient httpClient, ILogger<InsightsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<InsightInfo>> GetActiveInsightsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<InsightApiDto>>(
                "api/insights", cancellationToken);
            return dtos?.Where(d => !d.IsDismissed).Select(d => d.ToModel()).ToList() ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch insights");
            return [];
        }
    }

    public async Task<bool> DismissInsightAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsync(
                $"api/insights/{id}/dismiss",
                content: null,
                cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to dismiss insight {InsightId}", id);
            return false;
        }
    }
}
