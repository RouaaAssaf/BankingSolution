
using Microsoft.AspNetCore.Http;

public class DomainException : Exception
{
    public int StatusCode { get; }

    public DomainException(string message, int statusCode = StatusCodes.Status400BadRequest) : base(message)
    {
        StatusCode = statusCode;
    }
}
