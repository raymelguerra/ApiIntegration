using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DependencyInjections
{
    public static class EfExtensions
    {
        public static IServiceCollection AddEfInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<SyncDbContext>((serviceProvider, options) =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("DatabaseConnection");
                
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(SyncDbContext).Assembly.FullName);
                    
                    // Configure database connection resilience policy
                    // Retries 3 times with exponential backoff if connection fails
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null
                    );
                    
                    // Configure command timeout
                    npgsqlOptions.CommandTimeout(30);
                })
                .UseLoggerFactory(loggerFactory)
                .EnableSensitiveDataLogging(false)
                .ConfigureWarnings(warnings =>
                {
                    // Suppress detailed connection error logs to avoid stacktraces
                    warnings.Log(
                        (RelationalEventId.ConnectionOpening, LogLevel.Debug),
                        (RelationalEventId.ConnectionOpened, LogLevel.Debug),
                        (RelationalEventId.ConnectionClosing, LogLevel.Debug),
                        (RelationalEventId.ConnectionClosed, LogLevel.Debug)
                    );
                })
                .LogTo(
                    message =>
                    {
                        // Filter only retry strategy messages without including stacktrace
                        if (message.Contains("Execution strategy", StringComparison.OrdinalIgnoreCase))
                        {
                            if (message.Contains("retrying") || message.Contains("retry"))
                            {
                                logger.LogWarning("Retrying database connection...");
                            }
                        }
                        else if (message.Contains("opened connection", StringComparison.OrdinalIgnoreCase))
                        {
                            logger.LogInformation("Database connection established successfully");
                        }
                        else if (message.Contains("opening connection", StringComparison.OrdinalIgnoreCase))
                        {
                            logger.LogInformation("Attempting to connect to database...");
                        }
                    },
                    LogLevel.Information
                );
            });

            return services;
        }
    }
}