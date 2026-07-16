namespace ThrottleWatch.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddThrottleWatchOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        return services;
    }

    public static WebApplication UseThrottleWatchOpenApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        return app;
    }
}
