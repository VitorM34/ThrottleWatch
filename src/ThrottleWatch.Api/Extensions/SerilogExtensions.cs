using Serilog;

namespace ThrottleWatch.Api.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddThrottleWatchSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", "ThrottleWatch.Api");
        });

        return builder;
    }

    public static WebApplication UseThrottleWatchSerilogRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        return app;
    }
}
