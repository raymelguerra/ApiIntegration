using Application.Abstractions;
using Infrastructure.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Infrastructure.DependencyInjections
{
    public static class MediatorExtensions
    {
        public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddScoped<ISender, Sender>();

            assemblies = assemblies.Any() ? assemblies : [Assembly.GetCallingAssembly()];

            RegisterHandlers(services, assemblies);

            return services;
        }
        
        private static void RegisterHandlers(IServiceCollection services, Assembly[] assemblies)
        {
            var handlerTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    type.IsClass &&
                    !type.IsAbstract &&
                    type.GetInterfaces().Any(
                    i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .Select(t => new {
                    Implementation = t,
                    Interface = t.GetInterfaces()
                        .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                });

            foreach (var handler in handlerTypes)
            {
                services.AddTransient(handler.Interface, handler.Implementation);
            }
        }
    }
}