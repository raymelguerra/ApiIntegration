using Polly;
using Polly.Extensions.Http;

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
    }
}