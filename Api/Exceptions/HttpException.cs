using System.Net;

namespace Api.Exceptions
{
    public class HttpException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ErrorCode { get; }

        protected HttpException(HttpStatusCode statusCode, string message, string errorCode) 
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

    public class BadRequestException(string message, string errorCode = "BAD_REQUEST") : HttpException(HttpStatusCode.BadRequest, message, errorCode);

    public class NotFoundException(string message, string errorCode = "NOT_FOUND") : HttpException(HttpStatusCode.NotFound, message, errorCode);

    public class ConflictException(string message, string errorCode = "CONFLICT") : HttpException(HttpStatusCode.Conflict, message, errorCode);

    public class InternalServerErrorException : HttpException
    {
        public InternalServerErrorException(string message = "An internal server error occurred.", string errorCode = "INTERNAL_SERVER_ERROR") 
            : base(HttpStatusCode.InternalServerError, message, errorCode)
        {
        }
    }

    public class ServiceUnavailableException(string message = "Service is temporarily unavailable.", string errorCode = "SERVICE_UNAVAILABLE") : HttpException(HttpStatusCode.ServiceUnavailable, message, errorCode);
}
