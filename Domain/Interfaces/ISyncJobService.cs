namespace Domain.Interfaces
{
    public interface ISyncJobService
    {
        Task ExecuteJobAsync(string jobKey, CancellationToken cancellationToken);
        
    }
}