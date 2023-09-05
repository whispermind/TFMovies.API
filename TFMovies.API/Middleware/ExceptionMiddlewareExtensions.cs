using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;
using TFMovies.API.Common.Constants;
using TFMovies.API.Exceptions;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Middleware;

public static class ExceptionMiddlewareExtensions
{    
    public static void UseCustomExceptionHandler(this IApplicationBuilder app, ILogger logger)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                if (exceptionFeature != null)
                {
                    HttpStatusCode statusCode;
                    string logMessage = GetLogMessage(context, exceptionFeature);
                    var errorMessage = exceptionFeature.Error.Message;

                    switch (exceptionFeature.Error)
                    {
                        case ServiceException serviceException:
                            statusCode = serviceException.StatusCode;
                            break;
                        case UnauthorizedAccessException:
                            statusCode = HttpStatusCode.Unauthorized;
                            break;
                        case KeyNotFoundException:
                            statusCode = HttpStatusCode.NotFound;
                            break;
                        default:
                            statusCode = HttpStatusCode.InternalServerError;
                            errorMessage = ErrorMessages.UnexpectedError;
                            break;
                    }

                    logger.LogError(logMessage);
                    await WriteExceptionResponseAsync(context, errorMessage, statusCode);                   
                }
            });
        });
    }

    private static string GetLogMessage(HttpContext context, IExceptionHandlerPathFeature exceptionFeature)
    {
        var user = context.User.Identity?.Name ?? "Anonymous";
        var time = DateTime.UtcNow;
        return $"An error occurred for user '{user}' on path '{exceptionFeature.Path}' at UTC time '{time}'.";
    }

    private static async Task WriteExceptionResponseAsync(HttpContext context, string errorMessage, HttpStatusCode statusCode)
    {
        var response = context.Response;
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var result = JsonSerializer.Serialize(new ErrorResponse { ErrorMessage = errorMessage }, options);

        response.ContentType = "application/json";
        response.StatusCode = (int)statusCode;

        await response.WriteAsync(result);
    }
}