namespace Application.Abstractions
{
    public interface ISchedulerService
    {
        Task ScheduleOneTimeJobAsync(string jobKey, DateTime runAt, CancellationToken ct = default);
        
        // validate cron expression is valid
        bool ValidateCronExpression(string cronExpression, CancellationToken ct = default);
    }
}