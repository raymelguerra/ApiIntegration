using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Api.Exceptions;
using AppExceptions = Application.Exceptions;

namespace Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
        {
            // API Layer Exceptions
            HttpException httpEx => CreateProblemDetails(
                context,
                (int)httpEx.StatusCode,
                httpEx.ErrorCode,
                httpEx.Message,
                exception),

            // Application Layer Exceptions - Validation
            AppExceptions.ValidationException validationEx => CreateValidationProblemDetails(
                context,
                validationEx),

            AppExceptions.UnauthorizedException appEx => CreateProblemDetails(
                context,
                (int)HttpStatusCode.Unauthorized,
                appEx.ErrorCode,
                appEx.Message,
                exception),

            AppExceptions.ForbiddenException appEx => CreateProblemDetails(
                context,
                (int)HttpStatusCode.Forbidden,
                appEx.ErrorCode,
                appEx.Message,
                exception),

            AppExceptions.SyncJobException appEx => CreateProblemDetails(
                context,
                (int)HttpStatusCode.InternalServerError,
                appEx.ErrorCode,
                appEx.Message,
                exception),

            AppExceptions.CommandHandlerException appEx => CreateProblemDetails(
                context,
                (int)HttpStatusCode.InternalServerError,
                appEx.ErrorCode,
                appEx.Message,
                exception),

            AppExceptions.QueryHandlerException appEx => CreateProblemDetails(
                context,
                (int)HttpStatusCode.InternalServerError,
                appEx.ErrorCode,
                appEx.Message,
                exception),

            AppExceptions.ApplicationException appEx => CreateProblemDetails(
                context,
                (int)HttpStatusCode.BadRequest,
                appEx.ErrorCode,
                appEx.Message,
                exception),

            // Infrastructure Layer Exceptions - handled via reflection to avoid direct reference
            _ when exception.GetType().FullName?.StartsWith("Infrastructure.Exceptions") == true 
                => HandleInfrastructureException(context, exception),

            // Domain Layer Exceptions - handled via reflection to avoid direct reference
            _ when exception.GetType().FullName?.StartsWith("Domain.Exceptions") == true 
                => HandleDomainException(context, exception),

            // Generic Exceptions
            _ => CreateProblemDetails(
                context,
                (int)HttpStatusCode.InternalServerError,
                "INTERNAL_SERVER_ERROR",
                "An unexpected error occurred.",
                exception)
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }

    private ProblemDetails HandleDomainException(HttpContext context, Exception exception)
    {
        var exceptionTypeName = exception.GetType().Name;
        var errorCode = GetErrorCode(exception);
        
        var statusCode = exceptionTypeName switch
        {
            "EntityNotFoundException" => (int)HttpStatusCode.NotFound,
            "EntityAlreadyExistsException" => (int)HttpStatusCode.Conflict,
            "BusinessRuleValidationException" => (int)HttpStatusCode.BadRequest,
            "InvalidEntityStateException" => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.BadRequest
        };

        return CreateProblemDetails(context, statusCode, errorCode, exception.Message, exception);
    }

    private ProblemDetails HandleInfrastructureException(HttpContext context, Exception exception)
    {
        var exceptionTypeName = exception.GetType().Name;
        var errorCode = GetErrorCode(exception);
        
        var statusCode = exceptionTypeName switch
        {
            "CircuitBreakerOpenException" => (int)HttpStatusCode.ServiceUnavailable,
            "TimeoutException" => (int)HttpStatusCode.RequestTimeout,
            "ExternalApiException" => GetExternalApiStatusCode(exception),
            "DatabaseException" => (int)HttpStatusCode.InternalServerError,
            "QuartzJobException" => (int)HttpStatusCode.InternalServerError,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var message = exceptionTypeName == "DatabaseException" 
            ? "A database error occurred." 
            : exception.Message;

        return CreateProblemDetails(context, statusCode, errorCode, message, exception);
    }

    private static string GetErrorCode(Exception exception)
    {
        var property = exception.GetType().GetProperty("ErrorCode");
        return property?.GetValue(exception)?.ToString() ?? "UNKNOWN_ERROR";
    }

    private static int GetExternalApiStatusCode(Exception exception)
    {
        var property = exception.GetType().GetProperty("StatusCode");
        var statusCode = property?.GetValue(exception) as int?;
        return statusCode ?? (int)HttpStatusCode.BadGateway;
    }

    private ProblemDetails CreateProblemDetails(
        HttpContext context,
        int statusCode,
        string errorCode,
        string detail,
        Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = GetTitleForStatusCode(statusCode),
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["errorCode"] = errorCode;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        // Include stack trace and inner exception details only in development
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            
            if (exception.InnerException != null)
            {
                problemDetails.Extensions["innerException"] = new
                {
                    message = exception.InnerException.Message,
                    stackTrace = exception.InnerException.StackTrace
                };
            }
        }

        return problemDetails;
    }

    private ProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        AppExceptions.ValidationException validationException)
    {
        var problemDetails = new ValidationProblemDetails(validationException.Errors)
        {
            Type = "https://httpstatuses.com/400",
            Title = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = validationException.Message,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["errorCode"] = validationException.ErrorCode;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        return problemDetails;
    }

    private static string GetTitleForStatusCode(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        408 => "Request Timeout",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        500 => "Internal Server Error",
        502 => "Bad Gateway",
        503 => "Service Unavailable",
        504 => "Gateway Timeout",
        _ => "Error"
    };
}
