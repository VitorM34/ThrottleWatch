using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using ThrottleWatch.Client.Configuration;
using ThrottleWatch.Dashboard.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddThrottleWatch();
builder.Services.AddThrottleWatchDashboard(builder.Configuration);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("strict", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ResolvePartition(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(10),
                QueueLimit = 0
            }));

    options.AddPolicy("standard", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ResolvePartition(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromSeconds(10),
                QueueLimit = 0
            }));

    options.OnRejected = static (context, _) =>
    {
        ApplyPolicyHeader(context.HttpContext);
        context.HttpContext.Response.Headers.RetryAfter = "10";
        return ValueTask.CompletedTask;
    };
});

var app = builder.Build();

// Outer so 429s from the rate limiter are still captured.
app.UseThrottleWatch();
app.UseRateLimiter();
app.Use(async (context, next) =>
{
    ApplyPolicyHeader(context);
    await next();
});

app.UseThrottleWatchDashboard();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapGet("/api/orders", () => Results.Ok(new
    {
        items = new[]
        {
            new { id = 1, total = 42.50m },
            new { id = 2, total = 19.99m }
        }
    }))
    .RequireRateLimiting("strict");

app.MapGet("/api/products", () => Results.Ok(new
    {
        items = new[]
        {
            new { id = "sku-1", name = "Widget" },
            new { id = "sku-2", name = "Gadget" }
        }
    }))
    .RequireRateLimiting("standard");

app.MapPost("/api/orders", () => Results.Accepted())
    .RequireRateLimiting("strict");

app.Run();

static string ResolvePartition(HttpContext httpContext)
{
    if (httpContext.Request.Headers.TryGetValue("X-Api-Key", out var apiKey)
        && !string.IsNullOrWhiteSpace(apiKey))
    {
        return apiKey.ToString();
    }

    return httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
}

static void ApplyPolicyHeader(HttpContext httpContext)
{
    var policyName = httpContext.GetEndpoint()
        ?.Metadata
        .GetMetadata<EnableRateLimitingAttribute>()
        ?.PolicyName;

    if (!string.IsNullOrWhiteSpace(policyName))
        httpContext.Response.Headers["X-RateLimit-Policy"] = policyName;
}
