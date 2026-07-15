using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ThrottleWatch.Application.Services;
using ThrottleWatch.Application.Validators;

namespace ThrottleWatch.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMetricsService, MetricsService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IInsightService, InsightService>();

        services.AddScoped<IValidator<DTOs.Alerts.CreateAlertRuleDto>, CreateAlertRuleValidator>();
        services.AddScoped<IValidator<DTOs.Alerts.UpdateAlertRuleDto>, UpdateAlertRuleValidator>();

        return services;
    }
}
