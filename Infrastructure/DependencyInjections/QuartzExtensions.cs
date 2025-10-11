using Application.Abstractions;

using Infrastructure.Quartz;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Quartz;

namespace Infrastructure.DependencyInjections
{
    public static class QuartzExtensions
    {
        public static IServiceCollection AddQuartzInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Bind configuration from appsettings
            var quartzConfig = configuration.GetSection(QuartzConfiguration.SectionName)
                .Get<QuartzConfiguration>() ?? new QuartzConfiguration();

            // Fallback to connection string if not specified in Quartz section
            if (string.IsNullOrWhiteSpace(quartzConfig.ConnectionString))
            {
                quartzConfig.ConnectionString = configuration.GetConnectionString("DefaultConnection")
                                                ?? throw new ArgumentException("Quartz connection string is not configured");
            }

            // Validate connection string
            ArgumentException.ThrowIfNullOrWhiteSpace(quartzConfig.ConnectionString, nameof(quartzConfig.ConnectionString));

            // Register configuration
            services.AddSingleton(quartzConfig);

            services.AddQuartz(options =>
            {

                options.UsePersistentStore(storeOptions =>
                {
                    storeOptions.UsePostgres(cfg =>
                    {
                        cfg.ConnectionString = quartzConfig.ConnectionString;
                        cfg.TablePrefix = quartzConfig.TablePrefix;
                    });

                    // Clustering configuration for horizontal scaling
                    if (quartzConfig.EnableClustering)
                    {
                        storeOptions.UseClustering(c =>
                        {
                            c.CheckinInterval = TimeSpan.FromSeconds(quartzConfig.ClusterCheckinIntervalSeconds);
                            c.CheckinMisfireThreshold = TimeSpan.FromSeconds(quartzConfig.ClusterMisfireThresholdSeconds);
                        });
                    }

                    storeOptions.UseNewtonsoftJsonSerializer();
                    storeOptions.UseProperties = true;
                    storeOptions.PerformSchemaValidation = quartzConfig.PerformSchemaValidation;
                });

                // Configure thread pool
                options.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = quartzConfig.MaxConcurrency;
                });

                // Register job listener for exception handling
                options.AddJobListener<QuartzJobExceptionListener>();
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = quartzConfig.WaitForJobsToComplete;
            });

            services.AddHostedService<QuartzScheduleBackgroundService>();
            services.AddSingleton<ISchedulerService, QuartzSchedulerService>();

            return services;
        }
    }
}