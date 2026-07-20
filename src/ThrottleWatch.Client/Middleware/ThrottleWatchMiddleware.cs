using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThrottleWatch.Client.Configuration;
using ThrottleWatch.Client.Http;
using ThrottleWatch.Client.Queue;

namespace ThrottleWatch.Client.Middleware;

public sealed class ThrottleWatchMiddleware
{
    private readonly RequestDelegate _next;

    public ThrottleWatchMiddleware(RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        LocalMetricBuffer buffer,
        IOptionsMonitor<ThrottleWatchOptions> optionsMonitor,
        ILogger<ThrottleWatchMiddleware> logger)
    {
        var options = optionsMonitor.CurrentValue;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            try
            {
                TryCapture(context, buffer, options, stopwatch.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Failed to capture ThrottleWatch metric.");
            }
        }
    }

    private static void TryCapture(
        HttpContext context,
        LocalMetricBuffer buffer,
        ThrottleWatchOptions options,
        double durationMs)
    {
        var statusCode = context.Response.StatusCode;

        if (options.CaptureOnlyBlocked && statusCode != StatusCodes.Status429TooManyRequests)
            return;

        var path = context.Request.Path.HasValue
            ? context.Request.Path.Value!
            : "/";

        string? clientIp = null;
        if (options.CaptureClientIp)
            clientIp = context.Connection.RemoteIpAddress?.ToString();

        string? apiKey = null;
        if (!string.IsNullOrWhiteSpace(options.ApiKeyHeaderName)
            && context.Request.Headers.TryGetValue(options.ApiKeyHeaderName, out var apiKeyValues))
        {
            apiKey = apiKeyValues.ToString();
            if (string.IsNullOrWhiteSpace(apiKey))
                apiKey = null;
        }

        string? policyName = null;
        if (!string.IsNullOrWhiteSpace(options.PolicyNameHeaderName)
            && context.Response.Headers.TryGetValue(options.PolicyNameHeaderName, out var policyValues))
        {
            policyName = policyValues.ToString();
            if (string.IsNullOrWhiteSpace(policyName))
                policyName = null;
        }

        var payload = new MetricPayload(
            Path: path,
            Method: context.Request.Method,
            StatusCode: statusCode,
            DurationMs: durationMs,
            Timestamp: DateTimeOffset.UtcNow,
            ClientIp: clientIp,
            PolicyName: policyName,
            ApiKey: apiKey);

        buffer.TryEnqueue(payload);
    }
}
