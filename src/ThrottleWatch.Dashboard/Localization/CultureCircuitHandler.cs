using System.Globalization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Localization;

namespace ThrottleWatch.Dashboard.Localization;

/// <summary>
/// Applies the request culture to the Blazor Server circuit (WebSocket),
/// matching the official cookie-based localization guidance.
/// </summary>
public sealed class CultureCircuitHandler : CircuitHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CultureCircuitHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        ApplyRequestCulture();
        return Task.CompletedTask;
    }

    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next)
    {
        return async context =>
        {
            ApplyRequestCulture();
            await next(context);
        };
    }

    private void ApplyRequestCulture()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var requestCulture = httpContext?.Features.Get<IRequestCultureFeature>()?.RequestCulture;
        if (requestCulture is null)
            return;

        CultureInfo.CurrentCulture = requestCulture.Culture;
        CultureInfo.CurrentUICulture = requestCulture.UICulture;
        CultureInfo.DefaultThreadCurrentCulture = requestCulture.Culture;
        CultureInfo.DefaultThreadCurrentUICulture = requestCulture.UICulture;
    }
}
