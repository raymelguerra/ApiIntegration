using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Exceptions;
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
            
            try
            {
                return await ctx.SyncSchedules.ToListAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DatabaseException("Failed to retrieve schedules", ex);
            }
        }
        
        public async Task<SyncSchedule> GetScheduleAsync(string jobKey, CancellationToken ct = default)
        {
            logger.LogInformation("Getting schedule for job {JobKey}", jobKey);
            
            try
            {
                var schedule = await ctx.SyncSchedules.FirstOrDefaultAsync(x => x.JobKey == jobKey, ct);
                
                if (schedule == null)
                {
                    logger.LogWarning("Schedule not found for job {JobKey}", jobKey);
                    throw new DatabaseException($"SyncSchedule with key '{jobKey}' was not found");
                }
                
                return schedule;
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DatabaseException($"Failed to retrieve schedule for job '{jobKey}'", ex);
            }
        }
        
        public async Task UpsertScheduleAsync(SyncSchedule schedule, CancellationToken ct = default)
        {
            logger.LogInformation("Upserting schedule for job {JobKey}", schedule.JobKey);
            
            try
            {
                var existing = await ctx.SyncSchedules.FirstOrDefaultAsync(x => x.JobKey == schedule.JobKey, ct);
                
                if (existing == null)
                {
                    logger.LogWarning("Cannot update non-existent schedule for job {JobKey}", schedule.JobKey);
                    throw new DatabaseException($"SyncSchedule with key '{schedule.JobKey}' was not found");
                }
                
                existing.CronExpression = schedule.CronExpression;
                existing.Enabled = schedule.Enabled;
                existing.LastModifiedUtc = DateTime.UtcNow;
                ctx.SyncSchedules.Update(existing);
                await SaveChangesAsync(ct);
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DatabaseException($"Failed to upsert schedule for job '{schedule.JobKey}'", ex);
            }
        }

        public async Task AddExecutionHistoryAsync(ExecutionHistory h, CancellationToken ct = default)
        {
            logger.LogInformation("Adding execution history for job {JobKey}", h.JobKey);
            
            try
            {
                await ctx.ExecutionHistories.AddAsync(h, ct);
                await SaveChangesAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DatabaseException("Failed to add execution history", ex);
            }
        }

        public async Task AddFailedItemsAsync(IEnumerable<FailedItem> items, CancellationToken ct = default)
        {
            var failedItems = items as FailedItem[] ?? items.ToArray();
            logger.LogInformation("Adding {Count} failed items", failedItems.Length);
            
            try
            {
                await ctx.FailedItems.AddRangeAsync(failedItems, ct);
                await SaveChangesAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DatabaseException("Failed to add failed items", ex);
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Saving changes");
            
            try
            {
                return await ctx.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("Failed to save changes", ex);
            }
        }
    }
}