using Application.Services;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjections
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
            
            services.AddScoped<ISyncJobService, SyncJobService>();
            return services;
        }
    }
}