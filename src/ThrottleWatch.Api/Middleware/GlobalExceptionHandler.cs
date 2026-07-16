using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ThrottleWatch.Domain.Exceptions;

namespace ThrottleWatch.Api.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, logLevel) = exception switch
        {
            AlertRuleNotFoundException or AlertEventNotFoundException or InsightNotFoundException
                => (StatusCodes.Status404NotFound, "Resource not found", LogLevel.Warning),
            DomainException
                => (StatusCodes.Status400BadRequest, "Domain rule violation", LogLevel.Warning),
            FluentValidation.ValidationException
                => (StatusCodes.Status400BadRequest, "Validation failed", LogLevel.Warning),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", LogLevel.Error)
        };

        _logger.Log(logLevel, exception, "Exception for {Method} {Path}: {Message}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            exception.Message);

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception is FluentValidation.ValidationException validation
                ? string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        await _problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        return true;
    }
}
