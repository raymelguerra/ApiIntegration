namespace Infrastructure.Exceptions;

public abstract class InfrastructureException : Exception
{
    public string ErrorCode { get; }
    
    protected InfrastructureException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    protected InfrastructureException(string message, string errorCode, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

public class DatabaseException : InfrastructureException
{
    public DatabaseException(string message) 
        : base($"Database operation failed: {message}", "DATABASE_ERROR")
    {
    }

    public DatabaseException(string message, Exception innerException) 
        : base($"Database operation failed: {message}", "DATABASE_ERROR", innerException)
    {
    }
}

public class ExternalApiException : InfrastructureException
{
    public string ApiName { get; }
    public int? StatusCode { get; }

    public ExternalApiException(string apiName, string message) 
        : base($"External API '{apiName}' error: {message}", "EXTERNAL_API_ERROR")
    {
        ApiName = apiName;
    }

    public ExternalApiException(string apiName, int statusCode, string message) 
        : base($"External API '{apiName}' returned {statusCode}: {message}", "EXTERNAL_API_ERROR")
    {
        ApiName = apiName;
        StatusCode = statusCode;
    }

    public ExternalApiException(string apiName, string message, Exception innerException) 
        : base($"External API '{apiName}' error: {message}", "EXTERNAL_API_ERROR", innerException)
    {
        ApiName = apiName;
    }
}

public class QuartzJobException : InfrastructureException
{
    public string JobName { get; }

    public QuartzJobException(string jobName, string message) 
        : base($"Quartz job '{jobName}' failed: {message}", "QUARTZ_JOB_ERROR")
    {
        JobName = jobName;
    }

    public QuartzJobException(string jobName, string message, Exception innerException) 
        : base($"Quartz job '{jobName}' failed: {message}", "QUARTZ_JOB_ERROR", innerException)
    {
        JobName = jobName;
    }
}

public class CachingException(string message, Exception innerException) : InfrastructureException($"Caching operation failed: {message}", "CACHING_ERROR", innerException);

public class MessageBrokerException(string message, Exception innerException) : InfrastructureException($"Message broker operation failed: {message}", "MESSAGE_BROKER_ERROR", innerException);

public class CircuitBreakerOpenException(string serviceName) : InfrastructureException($"Circuit breaker is open for service '{serviceName}'. Service is temporarily unavailable.", "CIRCUIT_BREAKER_OPEN");

public class TimeoutException(string operation, TimeSpan timeout) : InfrastructureException($"Operation '{operation}' timed out after {timeout.TotalSeconds} seconds.", "OPERATION_TIMEOUT");
