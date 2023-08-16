using System.Net;
using System.Text.Json;
using TFMovies.API.Common.Constants;
using TFMovies.API.Exceptions;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ConflictException e) //custom
        {
            await LogAndHandleExceptionAsync(context, e, HttpStatusCode.Conflict);
        }
        catch (KeyNotFoundException e)
        {
            await LogAndHandleExceptionAsync(context, e, HttpStatusCode.NotFound);
        }
        catch (BadRequestException e) //custom
        {
            await LogAndHandleExceptionAsync(context, e, HttpStatusCode.BadRequest);
        }
        catch (UnauthorizedAccessException e)
        {
            await LogAndHandleExceptionAsync(context, e, HttpStatusCode.Unauthorized);
        }
        catch (InternalServerException e) //custom
        {
            await LogAndHandleExceptionAsync(context, e, HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            await LogAndHandleExceptionAsync(context, e, HttpStatusCode.InternalServerError, ErrorMessages.UnexpectedError);
        }
    }

    //Helpers
    private async Task LogAndHandleExceptionAsync(HttpContext context, Exception error, HttpStatusCode statusCode, string? defaultMessage = null)
    {
        var user = context.User.Identity?.Name ?? "Anonymous";
        var time = DateTime.UtcNow;
        var path = context.Request.Path;

        string errorDetails = string.Format("An error occurred for user '{0}' on path '{1}' at UTC time '{2}'.", user, path, time);

        if (error.InnerException != null)
        {
            errorDetails += $" | Inner Exception: {error.InnerException.Message}";
        }

        _logger.LogError(error, errorDetails);

        errorDetails += error.ToString();

        var response = context.Response;
        var message = defaultMessage ?? error.Message;
        var result = JsonSerializer.Serialize(new ErrorResponse { ErrorMessage = message, ErrorDetails = errorDetails });

        response.ContentType = "application/json";
        response.StatusCode = (int)statusCode;

        await response.WriteAsync(result);
    }
}
