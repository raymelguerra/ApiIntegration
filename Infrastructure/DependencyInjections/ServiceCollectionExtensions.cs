using Domain.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjections
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddEfInfrastructure(connectionString);
            services.AddHttpClientsInfrastructure();
            services.AddQuartzInfrastructure(connectionString);
            services.AddMediator();
            services.AddScoped<ISyncRepository, EfSyncRepository>();
            
            return services;
        }
    }
}