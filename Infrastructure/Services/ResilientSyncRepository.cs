using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Policies;
using Microsoft.Extensions.Logging;
using Polly;

namespace Infrastructure.Services
{
    /// <summary>
    /// Decorator that wraps ISyncRepository with resilience policies (retry + circuit breaker)
    /// This eliminates code duplication by applying policies globally to all database operations
    /// </summary>
    public class ResilientSyncRepository : ISyncRepository
    {
        private readonly ISyncRepository _innerRepository;
        private readonly IAsyncPolicy _combinedPolicy;

        public ResilientSyncRepository(
            ISyncRepository innerRepository, 
            ILogger<ResilientSyncRepository> logger)
        {
            _innerRepository = innerRepository;
            
            var retryPolicy = ResiliencePolicies.GetDatabaseRetryPolicy(logger);
            var circuitBreakerPolicy = ResiliencePolicies.GetDatabaseCircuitBreakerPolicy(logger);
            _combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
        }

        public Task<IEnumerable<SyncSchedule>> GetScheduleAsync(CancellationToken ct = default)
        {
            return _combinedPolicy.ExecuteAsync(() => _innerRepository.GetScheduleAsync(ct));
        }

        public Task<SyncSchedule> GetScheduleAsync(string jobKey, CancellationToken ct = default)
        {
            return _combinedPolicy.ExecuteAsync(() => _innerRepository.GetScheduleAsync(jobKey, ct));
        }

        public Task UpsertScheduleAsync(SyncSchedule schedule, CancellationToken ct = default)
        {
            return _combinedPolicy.ExecuteAsync(() => _innerRepository.UpsertScheduleAsync(schedule, ct));
        }

        public Task AddExecutionHistoryAsync(ExecutionHistory h, CancellationToken ct = default)
        {
            return _combinedPolicy.ExecuteAsync(() => _innerRepository.AddExecutionHistoryAsync(h, ct));
        }

        public Task AddFailedItemsAsync(IEnumerable<FailedItem> items, CancellationToken ct = default)
        {
            return _combinedPolicy.ExecuteAsync(() => _innerRepository.AddFailedItemsAsync(items, ct));
        }

        public Task<(IEnumerable<ExecutionHistory> Histories, int Count)> GetExecutionHistoryAsync(
            Paginator<HistorySortBy> filter, CancellationToken ct = default)
        {
            return _combinedPolicy.ExecuteAsync(() => _innerRepository.GetExecutionHistoryAsync(filter, ct));
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return _combinedPolicy.ExecuteAsync(() => _innerRepository.SaveChangesAsync(ct));
        }
    }
}
