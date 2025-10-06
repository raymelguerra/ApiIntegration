using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ISyncRepository
    {
        Task<IEnumerable<SyncSchedule>> GetScheduleAsync(CancellationToken ct = default);
        Task<SyncSchedule> GetScheduleAsync(string jobKey, CancellationToken ct = default);
        Task UpsertScheduleAsync(SyncSchedule schedule, CancellationToken ct = default);
        Task AddExecutionHistoryAsync(ExecutionHistory h, CancellationToken ct = default);
        Task AddFailedItemsAsync(IEnumerable<FailedItem> items, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}