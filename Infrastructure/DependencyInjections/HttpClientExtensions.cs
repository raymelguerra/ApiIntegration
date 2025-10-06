using Domain.Interfaces;
using Infrastructure.HttpClients;
using Infrastructure.Policies;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjections
{
    public static class HttpClientExtensions
    {
        public static IServiceCollection AddHttpClientsInfrastructure(this IServiceCollection services)
        {
            services.AddHttpClient<IA3ApiClient, A3ApiClient>()
                .AddPolicyHandler(HttpPolicies.GetHttpRetryPolicy())
                .AddPolicyHandler(HttpPolicies.GetHttpCircuitBreakerPolicy())
                .AddPolicyHandler(HttpPolicies.GetTimeoutPolicy(30));

            services.AddHttpClient<IGimApiClient, GimApiClient>()
                .AddPolicyHandler(HttpPolicies.GetHttpRetryPolicy())
                .AddPolicyHandler(HttpPolicies.GetHttpCircuitBreakerPolicy())
                .AddPolicyHandler(HttpPolicies.GetTimeoutPolicy(30));

            return services;
        }
    }
}