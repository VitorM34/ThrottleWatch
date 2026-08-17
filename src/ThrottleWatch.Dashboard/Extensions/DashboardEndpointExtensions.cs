using ThrottleWatch.Dashboard.Components;

namespace ThrottleWatch.Dashboard.Extensions;

public static class DashboardEndpointExtensions
{
    /// <summary>
    /// Localization, antiforgery, static assets, and Razor components.
    /// Default path is <c>/throttlewatch</c> (consumer). Pass <c>"/"</c> for the standalone host.
    /// </summary>
    /// <remarks>
    /// Prefixed hosting uses a branched pipeline so Dashboard routes do not steal the host app's
    /// <c>/health</c> (or other root paths). See
    /// <see href="https://learn.microsoft.com/aspnet/core/blazor/host-and-deploy/app-base-path">app base path</see>.
    /// </remarks>
    public static WebApplication UseThrottleWatchDashboard(
        this WebApplication app,
        string path = DashboardPath.DefaultPrefix)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseDashboardRequestLocalization();

        var prefix = DashboardPath.NormalizePrefix(path);
        if (prefix == "/")
        {
            app.UseAntiforgery();
            app.MapDashboardEndpoints();
            return app;
        }

        ((IApplicationBuilder)app).Map(prefix, subapp =>
        {
            subapp.UseRouting();
            subapp.UseAntiforgery();
            subapp.UseEndpoints(endpoints => endpoints.MapDashboardEndpoints());
        });

        return app;
    }

    private static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCultureSetEndpoint();
        endpoints.MapStaticAssets();
        endpoints.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return endpoints;
    }
}
