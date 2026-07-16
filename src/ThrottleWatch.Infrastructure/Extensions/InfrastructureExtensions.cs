using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Infrastructure.BackgroundServices;
using ThrottleWatch.Infrastructure.Events;
using ThrottleWatch.Infrastructure.Persistence;
using ThrottleWatch.Infrastructure.Persistence.Repositories;
using ThrottleWatch.Infrastructure.Queue;

namespace ThrottleWatch.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IMetricsRepository, MetricsRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IInsightRepository, InsightRepository>();

        services.AddSingleton<IMetricQueue, MetricQueue>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddHostedService<MetricProcessorService>();
        services.AddHostedService<AlertEvaluatorService>();
        services.AddHostedService<InsightGeneratorService>();
        services.AddHostedService<DataRetentionService>();

        return services;
    }
}
