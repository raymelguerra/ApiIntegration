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
                .AddPolicyHandler(HttpPolicies.GetTimeoutPolicy());

            services.AddHttpClient<IGimApiClient, GimApiClient>()
                .AddPolicyHandler(HttpPolicies.GetHttpRetryPolicy())
                .AddPolicyHandler(HttpPolicies.GetHttpCircuitBreakerPolicy())
                .AddPolicyHandler(HttpPolicies.GetTimeoutPolicy());

            return services;
        }
    }
}