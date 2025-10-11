using Application.Abstractions;
using Application.Services;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjections
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ISyncJobService, SyncJobService>();
            services.AddScoped<ISender, Sender>();
            
            // Register all request handlers from the Application assembly
            var assembly = typeof(ServiceCollectionExtensions).Assembly;
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => 
                    i.IsGenericType && 
                    i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .ToList();
            
            foreach (var handlerType in handlerTypes)
            {
                var interfaceType = handlerType.GetInterfaces()
                    .First(i => i.IsGenericType && 
                                i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
                services.AddScoped(interfaceType, handlerType);
            }
             
            return services;
        }
    }
}