using System.Net;

namespace Api.Exceptions;

public class HttpException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ErrorCode { get; }

    public HttpException(HttpStatusCode statusCode, string message, string errorCode) 
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public HttpException(HttpStatusCode statusCode, string message, string errorCode, Exception innerException) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

public class BadRequestException : HttpException
{
    public BadRequestException(string message, string errorCode = "BAD_REQUEST") 
        : base(HttpStatusCode.BadRequest, message, errorCode)
    {
    }
}

public class NotFoundException : HttpException
{
    public NotFoundException(string message, string errorCode = "NOT_FOUND") 
        : base(HttpStatusCode.NotFound, message, errorCode)
    {
    }
}

public class ConflictException : HttpException
{
    public ConflictException(string message, string errorCode = "CONFLICT") 
        : base(HttpStatusCode.Conflict, message, errorCode)
    {
    }
}

public class InternalServerErrorException : HttpException
{
    public InternalServerErrorException(string message = "An internal server error occurred.", string errorCode = "INTERNAL_SERVER_ERROR") 
        : base(HttpStatusCode.InternalServerError, message, errorCode)
    {
    }
}

public class ServiceUnavailableException : HttpException
{
    public ServiceUnavailableException(string message = "Service is temporarily unavailable.", string errorCode = "SERVICE_UNAVAILABLE") 
        : base(HttpStatusCode.ServiceUnavailable, message, errorCode)
    {
    }
}
