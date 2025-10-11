using Domain.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DependencyInjections
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentException("Connection string 'DefaultConnection' is not configured");
            
            services.AddEfInfrastructure(connectionString);
            services.AddHttpClientsInfrastructure();
            services.AddQuartzInfrastructure(configuration);
            
            // Register EfSyncRepository as the inner implementation
            services.AddScoped<EfSyncRepository>();
            
            // Register ResilientSyncRepository as a decorator that wraps EfSyncRepository
            // This applies resilience policies globally to all database operations
            services.AddScoped<ISyncRepository>(sp =>
            {
                var innerRepository = sp.GetRequiredService<EfSyncRepository>();
                var logger = sp.GetRequiredService<ILogger<ResilientSyncRepository>>();
                return new ResilientSyncRepository(innerRepository, logger);
            });
            
            return services;
        }
    }
}