namespace ThrottleWatch.Dashboard;

/// <summary>
/// Path helpers for hosting the dashboard at the app root or under a prefix
/// (<see href="https://learn.microsoft.com/aspnet/core/blazor/host-and-deploy/app-base-path">app base path</see>).
/// </summary>
public static class DashboardPath
{
    public const string DefaultPrefix = "/throttlewatch";

    public const string StaticContent = "_content/ThrottleWatch.Dashboard";

    public static string NormalizePrefix(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path == "/")
            return "/";

        return "/" + path.Trim('/');
    }

    /// <summary>
    /// Value for <c>&lt;base href&gt;</c>. Trailing slash is required by the Blazor docs.
    /// </summary>
    public static string ToBaseHref(string? pathBase)
    {
        if (string.IsNullOrEmpty(pathBase) || pathBase == "/")
            return "/";

        return pathBase.TrimEnd('/') + "/";
    }

    /// <summary>
    /// Root-absolute href for an RCL icon. CSS <c>mask: url()</c> via custom properties is
    /// resolved against <c>app.css</c>, so a relative <c>_content/...</c> path 404s.
    /// </summary>
    public static string IconHref(string? pathBase, string iconName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(iconName);

        var prefix = string.IsNullOrEmpty(pathBase) || pathBase == "/"
            ? string.Empty
            : pathBase.TrimEnd('/');

        return $"{prefix}/{StaticContent}/icons/{iconName}.svg";
    }
}
