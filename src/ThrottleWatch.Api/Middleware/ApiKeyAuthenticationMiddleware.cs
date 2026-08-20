using ThrottleWatch.Application.Tenancy;

namespace ThrottleWatch.Api.Middleware;

/// <summary>
/// Requires a shared API key on <c>/api/*</c> and stamps the matching tenant.
/// Leaves <c>/health</c> and other non-api paths open.
/// </summary>
public sealed class ApiKeyAuthenticationMiddleware
{
    public const string DefaultHeaderName = "X-ThrottleWatch-Key";

    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
    private readonly string _headerName;

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        IHostEnvironment environment,
        IApiKeyTenantMap tenantMap,
        ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;

        var configuredHeader = configuration["ThrottleWatch:Security:HeaderName"];
        _headerName = string.IsNullOrWhiteSpace(configuredHeader)
            ? DefaultHeaderName
            : configuredHeader.Trim();

        if (tenantMap.AuthEnabled)
            return;

        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "ThrottleWatch:Security:ApiKey (or ThrottleWatch:Security:Tenants) is required outside Development.");
        }

        _logger.LogWarning(
            "ThrottleWatch API key auth is disabled (no API keys configured in Development).");
    }

    /// <summary>
    /// Validates the shared API key for <c>/api/*</c> requests, stamps the tenant, then continues the pipeline.
    /// </summary>
    /// <remarks>Resolved by the ASP.NET Core pipeline by convention (no direct call sites).</remarks>
    // ReSharper disable once UnusedMember.Global
    public async Task InvokeAsync(
        HttpContext httpContext,
        ITenantContext tenantContext,
        IApiKeyTenantMap tenantMap)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (!IsProtectedPath(httpContext.Request.Path))
        {
            await _next(httpContext);
            return;
        }

        if (!tenantMap.AuthEnabled)
        {
            await _next(httpContext);
            return;
        }

        if (!httpContext.Request.Headers.TryGetValue(_headerName, out var provided)
            || !tenantMap.TryResolve(provided.ToString(), out var tenantId))
        {
            _logger.LogDebug("Rejected {Method} {Path}: missing or invalid API key.",
                httpContext.Request.Method, httpContext.Request.Path);
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await httpContext.Response.WriteAsJsonAsync(
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                    title = "Unauthorized",
                    status = 401,
                    detail = "A valid API key is required."
                },
                httpContext.RequestAborted);
            return;
        }

        tenantContext.Set(tenantId);
        await _next(httpContext);
    }

    private static bool IsProtectedPath(PathString path) =>
        path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
}
