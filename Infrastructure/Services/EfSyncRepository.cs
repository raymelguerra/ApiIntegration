using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class EfSyncRepository(SyncDbContext ctx, ILogger<EfSyncRepository> logger) : ISyncRepository
    {

        public async Task<IEnumerable<SyncSchedule>> GetScheduleAsync(CancellationToken ct = default)
        {
            logger.LogInformation("Getting schedules");
            return await ctx.SyncSchedules.ToListAsync(ct);
        }
        
        public async Task<SyncSchedule?> GetScheduleAsync(string jobKey, CancellationToken ct = default)
        {
            logger.LogInformation("Getting schedule for job {jobKey}", jobKey);
            return await ctx.SyncSchedules.FirstOrDefaultAsync(x => x.JobKey == jobKey, ct);
        }
        
        public async Task UpsertScheduleAsync(SyncSchedule schedule, CancellationToken ct = default)
        {
            logger.LogInformation("Upserting schedule for job {jobKey}", schedule.JobKey);
            var existing = await ctx.SyncSchedules.FirstOrDefaultAsync(x => x.JobKey == schedule.JobKey, ct);
            
            if (existing == null)
                throw new ArgumentNullException();
            
            existing.CronExpression = schedule.CronExpression;
            existing.Enabled = schedule.Enabled;
            existing.LastModifiedUtc = schedule.LastModifiedUtc;
            ctx.SyncSchedules.Update(existing);
            await SaveChangesAsync(ct);
        }

        public async Task AddExecutionHistoryAsync(ExecutionHistory h, CancellationToken ct = default)
        {
            logger.LogInformation("Adding execution history for job {jobKey}", h.JobKey);
            await ctx.ExecutionHistories.AddAsync(h, ct);
            await SaveChangesAsync(ct);
        }

        public async Task AddFailedItemsAsync(IEnumerable<FailedItem> items, CancellationToken ct = default)
        {
            var failedItems = items as FailedItem[] ?? items.ToArray();
            logger.LogInformation("Adding {count} failed items", failedItems.Length);
            await ctx.FailedItems.AddRangeAsync(failedItems, ct);
            await SaveChangesAsync(ct);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Saving changes");
            return await ctx.SaveChangesAsync(cancellationToken);
        }
    }
}