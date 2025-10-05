using Application.Abstractions;
using Infrastructure.Quartz;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure.DependencyInjections
{
    public static class QuartzExtensions
    {
        public static IServiceCollection AddQuartzInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddQuartz(options => {
                
                options.UsePersistentStore(storeOptions => {
                    storeOptions.UsePostgres(cfg => {
                        cfg.ConnectionString = connectionString;
                        cfg.TablePrefix = "quartz.qrtz_";
                    }
                    );
                    storeOptions.UseNewtonsoftJsonSerializer();
                    storeOptions.UseProperties = true;
                    storeOptions.PerformSchemaValidation = true;
                });
            });
            services.AddQuartzHostedService(options => {
                options.WaitForJobsToComplete = true;
            });
            services.AddHostedService<QuartzScheduleBackgroundService>();
            services.AddSingleton<ISchedulerService, QuartzSchedulerService>();
            
            return services;
        }
    }
}