using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Policies
{
    public static class HttpPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy(ILogger? logger = null)
        {
            // Retry 3 times with exponential backoff for transient 5xx or network errors
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => (int)msg.StatusCode == 429) // rate limit
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timeSpan, retryCount, _) =>
                    {
                        logger?.LogWarning(
                            outcome.Exception,
                            "HTTP request failed. Retry {RetryCount} after {RetryDelay}s. Status: {StatusCode}",
                            retryCount,
                            timeSpan.TotalSeconds,
                            outcome.Result?.StatusCode);
                    });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetHttpCircuitBreakerPolicy(ILogger? logger = null)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, duration) =>
                    {
                        logger?.LogError(
                            outcome.Exception,
                            "HTTP circuit breaker opened for {BreakDuration}s. Status: {StatusCode}",
                            duration.TotalSeconds,
                            outcome.Result?.StatusCode);
                    },
                    onReset: () =>
                    {
                        logger?.LogInformation("HTTP circuit breaker reset");
                    },
                    onHalfOpen: () =>
                    {
                        logger?.LogInformation("HTTP circuit breaker is half-open");
                    });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int timeoutSeconds = 30)
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(timeoutSeconds),
                TimeoutStrategy.Optimistic);
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy(ILogger? logger = null)
        {
            var retry = GetHttpRetryPolicy(logger);
            var circuitBreaker = GetHttpCircuitBreakerPolicy(logger);
            var timeout = GetTimeoutPolicy();

            // Apply policies in order: timeout -> retry -> circuit breaker
            return Policy.WrapAsync(timeout, retry, circuitBreaker);
        }
    }
}