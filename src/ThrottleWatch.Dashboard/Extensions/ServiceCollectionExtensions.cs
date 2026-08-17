using ApexCharts;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using ThrottleWatch.Dashboard.Localization;
using ThrottleWatch.Dashboard.Models;
using ThrottleWatch.Dashboard.Services;

namespace ThrottleWatch.Dashboard.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddThrottleWatchDashboard(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddOptions<ThrottleWatchOptions>()
            .Bind(configuration.GetSection(ThrottleWatchOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        void ConfigureApiClient(IServiceProvider sp, HttpClient client)
        {
            var options = sp.GetRequiredService<IOptionsMonitor<ThrottleWatchOptions>>().CurrentValue;
            client.BaseAddress = new Uri(options.ApiBaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                client.DefaultRequestHeaders.Remove("X-ThrottleWatch-Key");
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    "X-ThrottleWatch-Key",
                    options.ApiKey);
            }
        }

        services.AddHttpClient<IMetricsService, MetricsService>(ConfigureApiClient);
        services.AddHttpClient<IAlertsService, AlertsService>(ConfigureApiClient);
        services.AddHttpClient<IInsightsService, InsightsService>(ConfigureApiClient);

        services.AddScoped<IThemeService, ThemeService>();
        services.AddScoped<IToastService, ToastService>();

        services.AddLocalization(localization => localization.ResourcesPath = "Resources");
        services.AddHttpContextAccessor();
        services.AddScoped<CircuitHandler, CultureCircuitHandler>();

        var keysPath = configuration["DataProtection:KeysPath"];
        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            Directory.CreateDirectory(keysPath);
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
                .SetApplicationName("ThrottleWatch.Dashboard");
        }

        services.AddApexCharts();

        return services;
    }
}
