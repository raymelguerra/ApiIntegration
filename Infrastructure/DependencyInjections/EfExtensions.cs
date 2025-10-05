using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjections
{
    public static class EfExtensions
    {
        public static IServiceCollection AddEfInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<SyncDbContext>(options =>
                options.UseNpgsql(connectionString,
                b => b.MigrationsAssembly(typeof(SyncDbContext).Assembly.FullName)));

            return services;
        }
    }
}