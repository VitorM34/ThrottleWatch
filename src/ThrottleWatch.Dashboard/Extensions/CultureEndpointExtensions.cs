using Microsoft.AspNetCore.Localization;
using ThrottleWatch.Dashboard.Localization;

namespace ThrottleWatch.Dashboard.Extensions;

public static class CultureEndpointExtensions
{
    /// <summary>
    /// Sets the ASP.NET Core culture cookie and redirects (official Blazor pattern).
    /// </summary>
    public static WebApplication MapCultureEndpoints(this WebApplication app)
    {
        app.MapGet("/Culture/Set", (string culture, string redirectUri, HttpContext httpContext) =>
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

        return app;
    }

    public static WebApplication UseDashboardRequestLocalization(this WebApplication app)
    {
        var options = new RequestLocalizationOptions()
            .SetDefaultCulture(LocalizationConstants.DefaultCulture)
            .AddSupportedCultures(LocalizationConstants.SupportedCultures)
            .AddSupportedUICultures(LocalizationConstants.SupportedCultures);

        // Prefer cookie (Settings) over Accept-Language.
        options.RequestCultureProviders =
        [
            new CookieRequestCultureProvider(),
            new AcceptLanguageHeaderRequestCultureProvider()
        ];

        app.UseRequestLocalization(options);
        return app;
    }
}
