using Microsoft.AspNetCore.Localization;
using ThrottleWatch.Dashboard.Localization;

namespace ThrottleWatch.Dashboard.Extensions;

public static class CultureEndpointExtensions
{
    /// <summary>
    /// Sets the ASP.NET Core culture cookie and redirects (official Blazor cookie culture pattern).
    /// </summary>
    public static IEndpointRouteBuilder MapCultureSetEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/Culture/Set", (string culture, string redirectUri, HttpContext httpContext) =>
        {
            if (!string.IsNullOrWhiteSpace(culture)
                && LocalizationConstants.SupportedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase))
            {
                httpContext.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture, culture)),
                    new CookieOptions
                    {
                        Path = "/",
                        IsEssential = true,
                        HttpOnly = false,
                        Secure = httpContext.Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        MaxAge = TimeSpan.FromDays(365)
                    });
            }

            var target = string.IsNullOrWhiteSpace(redirectUri) ? "/" : redirectUri;
            return Results.LocalRedirect(target);
        });

        return endpoints;
    }

    public static IApplicationBuilder UseDashboardRequestLocalization(this IApplicationBuilder app)
    {
        var options = new RequestLocalizationOptions()
            .SetDefaultCulture(LocalizationConstants.DefaultCulture)
            .AddSupportedCultures(LocalizationConstants.SupportedCultures)
            .AddSupportedUICultures(LocalizationConstants.SupportedCultures);

        options.RequestCultureProviders =
        [
            new CookieRequestCultureProvider(),
            new AcceptLanguageHeaderRequestCultureProvider()
        ];

        app.UseRequestLocalization(options);
        return app;
    }
}
