using ApexCharts;
using ThrottleWatch.Dashboard.Models;
using ThrottleWatch.Dashboard.Services;

namespace ThrottleWatch.Dashboard.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddThrottleWatchDashboard(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ThrottleWatchOptions>()
            .Bind(configuration.GetSection(ThrottleWatchOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = configuration
            .GetSection(ThrottleWatchOptions.SectionName)
            .Get<ThrottleWatchOptions>() ?? new ThrottleWatchOptions();

        void ConfigureApiClient(HttpClient client)
        {
            client.BaseAddress = new Uri(options.ApiBaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        services.AddHttpClient<IMetricsService, MetricsService>(ConfigureApiClient);
        services.AddHttpClient<IAlertsService, AlertsService>(ConfigureApiClient);
        services.AddHttpClient<IInsightsService, InsightsService>(ConfigureApiClient);

        services.AddScoped<IThemeService, ThemeService>();
        services.AddScoped<IToastService, ToastService>();

        services.AddApexCharts();

        return services;
    }
}
