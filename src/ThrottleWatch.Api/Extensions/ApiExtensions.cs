using ThrottleWatch.Api.Middleware;
using ThrottleWatch.Application.Extensions;
using ThrottleWatch.Infrastructure.Extensions;

namespace ThrottleWatch.Api.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        services.AddApplication();
        services.AddInfrastructure(connectionString);

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
}
