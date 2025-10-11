using Domain.Entities;
using Domain.Enums;
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

        public async Task<(IEnumerable<ExecutionHistory> Histories, int Count)> GetExecutionHistoryAsync(Paginator<HistorySortBy> filter, CancellationToken ct = default)
        {
            logger.LogInformation("Getting execution history with filter: {@Filter}", filter);

            try
            {
                var query = ctx.ExecutionHistories.AsQueryable();

                // Get total count before pagination
                var totalCount = await query.CountAsync(ct);

                // Apply sorting
                query = filter.SortBy switch
                {
                    HistorySortBy.JobKey => filter.SortOrder == SortOrder.Ascending
                        ? query.OrderBy(h => h.JobKey)
                        : query.OrderByDescending(h => h.JobKey),
                    HistorySortBy.StartedAt => filter.SortOrder == SortOrder.Ascending
                        ? query.OrderBy(h => h.StartedAtUtc)
                        : query.OrderByDescending(h => h.StartedAtUtc),
                    HistorySortBy.FinishedAt => filter.SortOrder == SortOrder.Ascending
                        ? query.OrderBy(h => h.FinishedAtUtc)
                        : query.OrderByDescending(h => h.FinishedAtUtc),
                    HistorySortBy.ExtractedCount => filter.SortOrder == SortOrder.Ascending
                        ? query.OrderBy(h => h.ExtractedCount)
                        : query.OrderByDescending(h => h.ExtractedCount),
                    HistorySortBy.SuccessCount => filter.SortOrder == SortOrder.Ascending
                        ? query.OrderBy(h => h.SuccessCount)
                        : query.OrderByDescending(h => h.SuccessCount),
                    HistorySortBy.FailedCount => filter.SortOrder == SortOrder.Ascending
                        ? query.OrderBy(h => h.FailedCount)
                        : query.OrderByDescending(h => h.FailedCount),
                    _ => query.OrderByDescending(h => h.StartedAtUtc) // Default sorting
                };

                // Apply pagination
                if (filter.Offset.HasValue)
                {
                    query = query.Skip(filter.Offset.Value);
                }

                if (filter.Limit.HasValue)
                {
                    query = query.Take(filter.Limit.Value);
                }

                var histories = await query.ToListAsync(ct);

                return (histories, totalCount);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DatabaseException("Failed to retrieve execution history", ex);
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