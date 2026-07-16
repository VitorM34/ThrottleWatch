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

        services.AddHttpClient<IMetricsService, MetricsService>(client =>
        {
            client.BaseAddress = new Uri(options.ApiBaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddScoped<IThemeService, ThemeService>();
        services.AddScoped<IToastService, ToastService>();

        services.AddApexCharts();

        return services;
    }
}
