namespace Application.Exceptions;

public abstract class ApplicationException : Exception
{
    public string ErrorCode { get; }
    
    protected ApplicationException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    protected ApplicationException(string message, string errorCode, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

public class ValidationException : ApplicationException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors) 
        : base("One or more validation errors occurred.", "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error) 
        : base($"Validation failed for field '{field}': {error}", "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, [error] }
        };
    }
}

public abstract class UnauthorizedException(string message = "Unauthorized access.") : ApplicationException(message, "UNAUTHORIZED");

public abstract class ForbiddenException(string message = "Access forbidden.") : ApplicationException(message, "FORBIDDEN");

public abstract class CommandHandlerException(string commandName, Exception innerException) : ApplicationException($"Error executing command '{commandName}'.", "COMMAND_HANDLER_ERROR", innerException);

public abstract class QueryHandlerException(string queryName, Exception innerException) : ApplicationException($"Error executing query '{queryName}'.", "QUERY_HANDLER_ERROR", innerException);

public abstract class SyncJobException : ApplicationException
{
    public string JobType { get; }

    protected SyncJobException(string jobType, string message) 
        : base($"Sync job '{jobType}' failed: {message}", "SYNC_JOB_ERROR")
    {
        JobType = jobType;
    }

    protected SyncJobException(string jobType, string message, Exception innerException) 
        : base($"Sync job '{jobType}' failed: {message}", "SYNC_JOB_ERROR", innerException)
    {
        JobType = jobType;
    }
}

