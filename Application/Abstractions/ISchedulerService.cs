namespace Application.Abstractions
{
    public interface ISchedulerService
    {
        Task ScheduleOneTimeJobAsync(string jobKey, DateTime runAt, CancellationToken ct = default);
    }
}