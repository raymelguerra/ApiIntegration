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
            { field, new[] { error } }
        };
    }
}

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message = "Unauthorized access.") 
        : base(message, "UNAUTHORIZED")
    {
    }
}

public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message = "Access forbidden.") 
        : base(message, "FORBIDDEN")
    {
    }
}

public class CommandHandlerException : ApplicationException
{
    public CommandHandlerException(string commandName, Exception innerException) 
        : base($"Error executing command '{commandName}'.", "COMMAND_HANDLER_ERROR", innerException)
    {
    }
}

public class QueryHandlerException : ApplicationException
{
    public QueryHandlerException(string queryName, Exception innerException) 
        : base($"Error executing query '{queryName}'.", "QUERY_HANDLER_ERROR", innerException)
    {
    }
}

public class SyncJobException : ApplicationException
{
    public string JobType { get; }

    public SyncJobException(string jobType, string message) 
        : base($"Sync job '{jobType}' failed: {message}", "SYNC_JOB_ERROR")
    {
        JobType = jobType;
    }

    public SyncJobException(string jobType, string message, Exception innerException) 
        : base($"Sync job '{jobType}' failed: {message}", "SYNC_JOB_ERROR", innerException)
    {
        JobType = jobType;
    }
}

