using Microsoft.EntityFrameworkCore;
using ThrottleWatch.Api.Middleware;
using ThrottleWatch.Application.Extensions;
using ThrottleWatch.Infrastructure.Extensions;
using ThrottleWatch.Infrastructure.Persistence;

namespace ThrottleWatch.Api.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        EnsureApiKeyConfigured(configuration, environment);

        services.AddApplication();
        services.AddInfrastructure(connectionString, configuration);
        services.AddThrottleWatchTelemetry(configuration, environment);

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddHealthChecks();

        services.AddCors(options =>
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:5000", "https://localhost:5001", "http://localhost:5173"];

            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseThrottleWatchApiKeyAuth(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
    }

    public static async Task ApplyMigrationsIfConfiguredAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        var applyMigrations = app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", false);
        if (!applyMigrations)
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    private static void EnsureApiKeyConfigured(IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
            return;

        var apiKey = configuration["ThrottleWatch:Security:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "ThrottleWatch:Security:ApiKey is required outside Development.");
        }
    }
}
