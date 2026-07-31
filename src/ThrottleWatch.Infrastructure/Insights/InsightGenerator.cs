using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Events;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Infrastructure.Configuration;

namespace ThrottleWatch.Infrastructure.Insights;

public sealed class InsightGenerator : IInsightGenerator
{
    private readonly IEnumerable<IInsightAnalyzer> _analyzers;
    private readonly IMetricsRepository _metricsRepository;
    private readonly IInsightRepository _insightRepository;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly IOptionsMonitor<ThrottleWatchOptions> _options;
    private readonly ILogger<InsightGenerator> _logger;

    public InsightGenerator(
        IEnumerable<IInsightAnalyzer> analyzers,
        IMetricsRepository metricsRepository,
        IInsightRepository insightRepository,
        IDomainEventDispatcher dispatcher,
        IOptionsMonitor<ThrottleWatchOptions> options,
        ILogger<InsightGenerator> logger)
    {
        _analyzers = analyzers;
        _metricsRepository = metricsRepository;
        _insightRepository = insightRepository;
        _dispatcher = dispatcher;
        _options = options;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Insight>> GenerateAsync(CancellationToken ct)
    {
        var generated = new List<Insight>();
        var dedupWindow = TimeSpan.FromMinutes(
            Math.Clamp(_options.CurrentValue.Insights.DedupWindowMinutes, 1, 24 * 60));

        foreach (var analyzer in _analyzers)
        {
            try
            {
                var insights = await analyzer.AnalyzeAsync(_metricsRepository, ct);
                foreach (var insight in insights)
                {
                    var exists = await _insightRepository.ExistsRecentAsync(
                        insight.Type,
                        insight.AffectedResource,
                        dedupWindow,
                        ct);

                    if (exists)
                        continue;

                    await _insightRepository.AddAsync(insight, ct);
                    await _dispatcher.DispatchAsync(
                        new InsightGeneratedEvent(insight.Id, insight.Type, insight.Title),
                        ct);

                    generated.Add(insight);
                    _logger.LogInformation(
                        "Insight generated: {Type} — {Title}",
                        insight.Type,
                        insight.Title);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Insight analyzer {Analyzer} failed.",
                    analyzer.GetType().Name);
            }
        }

        return generated;
    }
}
