using Polly;
using Polly.Timeout;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Policies;

public static class ResiliencePolicies
{
    public static IAsyncPolicy GetDatabaseRetryPolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>(IsDatabaseTransientError)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, _) =>
                {
                    logger.LogWarning(
                        exception,
                        "Database operation failed. Retry {RetryCount} after {RetryDelay}s",
                        retryCount,
                        timeSpan.TotalSeconds);
                });
    }

    public static IAsyncPolicy GetDatabaseCircuitBreakerPolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>(IsDatabaseTransientError)
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (exception, duration) =>
                {
                    logger.LogError(
                        exception,
                        "Database circuit breaker opened for {BreakDuration}s",
                        duration.TotalSeconds);
                },
                onReset: () =>
                {
                    logger.LogInformation("Database circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Database circuit breaker is half-open");
                });
    }

    public static IAsyncPolicy GetTimeoutPolicy(TimeSpan timeout)
    {
        return Policy
            .TimeoutAsync(timeout, TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (_, _, _) => Task.CompletedTask);
    }

    public static IAsyncPolicy GetExternalApiRetryPolicy(ILogger logger, string apiName)
    {
        return Policy
            .Handle<ExternalApiException>()
            .Or<HttpRequestException>()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, _) =>
                {
                    logger.LogWarning(
                        exception,
                        "External API '{ApiName}' call failed. Retry {RetryCount} after {RetryDelay}s",
                        apiName,
                        retryCount,
                        timeSpan.TotalSeconds);
                });
    }

    public static IAsyncPolicy GetExternalApiCircuitBreakerPolicy(ILogger logger, string apiName)
    {
        return Policy
            .Handle<ExternalApiException>()
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    logger.LogError(
                        exception,
                        "Circuit breaker opened for external API '{ApiName}' for {BreakDuration}s",
                        apiName,
                        duration.TotalSeconds);
                },
                onReset: () =>
                {
                    logger.LogInformation(
                        "Circuit breaker reset for external API '{ApiName}'",
                        apiName);
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation(
                        "Circuit breaker is half-open for external API '{ApiName}'",
                        apiName);
                });
    }

    public static IAsyncPolicy GetQuartzJobRetryPolicy(ILogger logger, string jobName)
    {
        return Policy
            .Handle<Exception>(IsRetryableJobError)
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(5 * retryAttempt),
                onRetry: (exception, timeSpan, retryCount, _) =>
                {
                    logger.LogWarning(
                        exception,
                        "Quartz job '{JobName}' failed. Retry {RetryCount} after {RetryDelay}s",
                        jobName,
                        retryCount,
                        timeSpan.TotalSeconds);
                });
    }

    private static bool IsDatabaseTransientError(Exception ex)
    {
        // Check for known transient database errors
        var message = ex.Message.ToLower();
        return message.Contains("timeout") ||
               message.Contains("deadlock") ||
               message.Contains("connection") ||
               ex is DatabaseException;
    }

    private static bool IsRetryableJobError(Exception ex)
    {
        // Don't retry validation errors or business rule violations using reflection
        var exceptionTypeName = ex.GetType().FullName ?? "";
        
        if (exceptionTypeName.Contains("ValidationException") ||
            exceptionTypeName.Contains("BusinessRuleValidationException"))
        {
            return false;
        }

        // Retry infrastructure errors
        return ex is InfrastructureException ||
               ex is HttpRequestException ||
               ex is TimeoutRejectedException;
    }
}
