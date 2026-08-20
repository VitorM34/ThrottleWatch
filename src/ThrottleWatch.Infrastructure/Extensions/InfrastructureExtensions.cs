using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Application.Tenancy;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Infrastructure.Alerting;
using ThrottleWatch.Infrastructure.Alerting.Notifiers;
using ThrottleWatch.Infrastructure.BackgroundServices;
using ThrottleWatch.Infrastructure.Configuration;
using ThrottleWatch.Infrastructure.Events;
using ThrottleWatch.Infrastructure.Insights;
using ThrottleWatch.Infrastructure.Insights.Analyzers;
using ThrottleWatch.Infrastructure.Persistence;
using ThrottleWatch.Infrastructure.Persistence.Repositories;
using ThrottleWatch.Infrastructure.Queue;
using ThrottleWatch.Infrastructure.Security;

namespace ThrottleWatch.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<ThrottleWatchOptions>(
            configuration.GetSection(ThrottleWatchOptions.SectionName));

        var security = configuration.GetSection($"{ThrottleWatchOptions.SectionName}:Security")
            .Get<SecurityOptions>() ?? new SecurityOptions();
        _ = ApiKeyTenantMap.BuildEntries(security);

        services.AddSingleton<IApiKeyTenantMap, ApiKeyTenantMap>();

        services.AddScoped<IMetricsRepository, MetricsRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IInsightRepository, InsightRepository>();

        services.AddSingleton<IMetricQueue, MetricQueue>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddHttpClient(WebhookNotifier.HttpClientName, client =>
            client.Timeout = TimeSpan.FromSeconds(10));
        services.AddHttpClient(SlackNotifier.HttpClientName, client =>
            client.Timeout = TimeSpan.FromSeconds(10));
        services.AddHttpClient(DiscordNotifier.HttpClientName, client =>
            client.Timeout = TimeSpan.FromSeconds(10));

        services.AddSingleton<IAlertNotifier, WebhookNotifier>();
        services.AddSingleton<IAlertNotifier, SlackNotifier>();
        services.AddSingleton<IAlertNotifier, DiscordNotifier>();
        services.AddSingleton<IAlertNotifier, EmailNotifier>();
        services.AddScoped<AlertNotificationService>();

        services.AddScoped<IInsightAnalyzer, HighBlockRateAnalyzer>();
        services.AddScoped<IInsightAnalyzer, SuspiciousClientAnalyzer>();
        services.AddScoped<IInsightAnalyzer, MisconfiguredPolicyAnalyzer>();
        services.AddScoped<IInsightAnalyzer, PeakHoursAnalyzer>();
        services.AddScoped<IInsightGenerator, InsightGenerator>();

        services.AddHostedService<MetricProcessorService>();
        services.AddHostedService<AlertEvaluatorService>();
        services.AddHostedService<InsightGeneratorService>();
        services.AddHostedService<MetricsRollupService>();
        services.AddHostedService<DataRetentionService>();

        return services;
    }
}
