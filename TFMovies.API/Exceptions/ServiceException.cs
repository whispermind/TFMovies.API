using Microsoft.AspNetCore.Http;
using System.Net;

namespace TFMovies.API.Exceptions;

public class ServiceException : Exception
{
    public HttpStatusCode StatusCode { get; }   

    public ServiceException(HttpStatusCode statusCode)
    { 
        StatusCode = statusCode;
    }
    public ServiceException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }    
}
