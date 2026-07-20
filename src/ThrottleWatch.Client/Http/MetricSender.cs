using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThrottleWatch.Client.Configuration;
using ThrottleWatch.Client.Queue;

namespace ThrottleWatch.Client.Http;

public sealed class MetricSender : BackgroundService
{
    public const string HttpClientName = "ThrottleWatch";

    private readonly LocalMetricBuffer _buffer;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<ThrottleWatchOptions> _options;
    private readonly ILogger<MetricSender> _logger;

    public MetricSender(
        LocalMetricBuffer buffer,
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<ThrottleWatchOptions> options,
        ILogger<MetricSender> logger)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _buffer = buffer;
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ThrottleWatch MetricSender started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _options.CurrentValue;
            var delay = TimeSpan.FromMilliseconds(options.FlushIntervalMilliseconds);

            try
            {
                await FlushAsync(options.BatchSize, stoppingToken);
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to flush metrics to ThrottleWatch.Api.");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        try
        {
            await FlushAsync(_options.CurrentValue.BatchSize, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Final metric flush failed during shutdown.");
        }

        _logger.LogInformation("ThrottleWatch MetricSender stopped.");
    }

    private async Task FlushAsync(int batchSize, CancellationToken ct)
    {
        while (true)
        {
            var batch = _buffer.DequeueBatch(batchSize);
            if (batch.Count == 0)
                return;

            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.PostAsJsonAsync("api/metrics", batch, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "ThrottleWatch.Api returned {StatusCode} when posting {Count} metrics.",
                    (int)response.StatusCode,
                    batch.Count);
            }
            else
            {
                _logger.LogDebug("Posted {Count} metrics to ThrottleWatch.Api.", batch.Count);
            }

            if (batch.Count < batchSize)
                return;
        }
    }
}
