using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DependencyInjections;

public static class DatabaseMigrationExtensions
{
    /// <summary>
    ///     Applies pending database migrations automatically on application startup.
    ///     This method should be called after building the application host.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve dependencies</param>
    /// <returns>The same service provider for chaining</returns>
    public static IServiceProvider ApplyDatabaseMigrations(this IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        try
        {
            SyncDbContext dbContext = services.GetRequiredService<SyncDbContext>();
            ILogger<SyncDbContext> logger = services.GetRequiredService<ILogger<SyncDbContext>>();

            logger.LogInformation("Applying database migrations...");
            dbContext.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            ILogger<SyncDbContext> logger = services.GetRequiredService<ILogger<SyncDbContext>>();
            logger.LogError(ex, "An error occurred while applying database migrations.");
            throw;
        }

        return serviceProvider;
    }
}