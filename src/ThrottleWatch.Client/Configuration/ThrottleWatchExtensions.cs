using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ThrottleWatch.Client.Http;
using ThrottleWatch.Client.Middleware;
using ThrottleWatch.Client.Queue;

namespace ThrottleWatch.Client.Configuration;

public static class ThrottleWatchExtensions
{
    public static IServiceCollection AddThrottleWatch(this IServiceCollection services)
    {
        return services.AddThrottleWatch(ThrottleWatchOptions.SectionName);
    }

    public static IServiceCollection AddThrottleWatch(
        this IServiceCollection services,
        string configSectionPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configSectionPath);

        services.AddOptions<ThrottleWatchOptions>()
            .BindConfiguration(configSectionPath)
            .ValidateDataAnnotations()
            .Validate(
                static o => Uri.TryCreate(o.ApiBaseUrl, UriKind.Absolute, out _),
                $"{ThrottleWatchOptions.SectionName}:ApiBaseUrl must be an absolute URL.")
            .ValidateOnStart();

        RegisterCoreServices(services);
        return services;
    }

    public static IServiceCollection AddThrottleWatch(
        this IServiceCollection services,
        IConfiguration namedConfigurationSection)
    {
        ArgumentNullException.ThrowIfNull(namedConfigurationSection);

        services.AddOptions<ThrottleWatchOptions>()
            .Bind(namedConfigurationSection)
            .ValidateDataAnnotations()
            .Validate(
                static o => Uri.TryCreate(o.ApiBaseUrl, UriKind.Absolute, out _),
                $"{ThrottleWatchOptions.SectionName}:ApiBaseUrl must be an absolute URL.")
            .ValidateOnStart();

        RegisterCoreServices(services);
        return services;
    }

    public static IServiceCollection AddThrottleWatch(
        this IServiceCollection services,
        Action<ThrottleWatchOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddOptions<ThrottleWatchOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .Validate(
                static o => Uri.TryCreate(o.ApiBaseUrl, UriKind.Absolute, out _),
                $"{ThrottleWatchOptions.SectionName}:ApiBaseUrl must be an absolute URL.")
            .ValidateOnStart();

        RegisterCoreServices(services);
        return services;
    }

    public static IApplicationBuilder UseThrottleWatch(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<ThrottleWatchMiddleware>();
    }

    private static void RegisterCoreServices(IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ThrottleWatchOptions>>().Value;
            return new LocalMetricBuffer(options.BufferCapacity);
        });

        services.AddHttpClient(MetricSender.HttpClientName, static (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<ThrottleWatchOptions>>().CurrentValue;
                client.BaseAddress = new Uri(options.ApiBaseUrl.TrimEnd('/') + "/");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            .AddStandardResilienceHandler();

        services.AddHostedService<MetricSender>();
    }
}
