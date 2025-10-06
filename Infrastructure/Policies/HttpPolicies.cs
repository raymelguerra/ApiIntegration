using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace Infrastructure.Policies
{
    public static class HttpPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy()
        {
            // Retry 3 times with exponential backoff for transient 5xx or network errors
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => (int)msg.StatusCode == 429) // rate limit
                .WaitAndRetryAsync(new[] {
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetHttpCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (_, _) =>
                    {
                        // Log circuit breaker opening
                    },
                    onReset: () =>
                    {
                        // Log circuit breaker reset
                    },
                    onHalfOpen: () =>
                    {
                        // Log circuit breaker half-open
                    });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int timeoutSeconds = 30)
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(timeoutSeconds),
                TimeoutStrategy.Optimistic);
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
        {
            var retry = GetHttpRetryPolicy();
            var circuitBreaker = GetHttpCircuitBreakerPolicy();
            var timeout = GetTimeoutPolicy();

            // Apply policies in order: timeout -> retry -> circuit breaker
            return Policy.WrapAsync(timeout, retry, circuitBreaker);
        }
    }
}