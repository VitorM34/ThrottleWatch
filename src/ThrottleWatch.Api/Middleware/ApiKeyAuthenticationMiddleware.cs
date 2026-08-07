using System.Security.Cryptography;
using System.Text;

namespace ThrottleWatch.Api.Middleware;

/// <summary>
/// Requires a shared API key on <c>/api/*</c>. Leaves <c>/health</c> and other non-api paths open.
/// </summary>
public sealed class ApiKeyAuthenticationMiddleware
{
    public const string DefaultHeaderName = "X-ThrottleWatch-Key";

    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
    private readonly bool _authEnabled;
    private readonly string _expectedKey;
    private readonly string _headerName;

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;

        var configuredHeader = configuration["ThrottleWatch:Security:HeaderName"];
        _headerName = string.IsNullOrWhiteSpace(configuredHeader)
            ? DefaultHeaderName
            : configuredHeader.Trim();

        var key = configuration["ThrottleWatch:Security:ApiKey"]?.Trim();
        if (string.IsNullOrEmpty(key))
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "ThrottleWatch:Security:ApiKey is required outside Development.");
            }

            _authEnabled = false;
            _expectedKey = string.Empty;
            _logger.LogWarning(
                "ThrottleWatch API key auth is disabled (empty ThrottleWatch:Security:ApiKey in Development).");
        }
        else
        {
            _authEnabled = true;
            _expectedKey = key;
        }
    }

    /// <summary>
    /// Validates the shared API key for <c>/api/*</c> requests, then continues the pipeline.
    /// </summary>
    /// <remarks>Resolved by the ASP.NET Core pipeline by convention (no direct call sites).</remarks>
    // ReSharper disable once UnusedMember.Global
    public async Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (!_authEnabled || !IsProtectedPath(httpContext.Request.Path))
        {
            await _next(httpContext);
            return;
        }

        if (!httpContext.Request.Headers.TryGetValue(_headerName, out var provided)
            || !FixedTimeEquals(provided.ToString(), _expectedKey))
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

        await _next(httpContext);
    }

    private static bool IsProtectedPath(PathString path) =>
        path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);

    private static bool FixedTimeEquals(string provided, string expected)
    {
        var a = Encoding.UTF8.GetBytes(provided);
        var b = Encoding.UTF8.GetBytes(expected);
        if (a.Length != b.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}
