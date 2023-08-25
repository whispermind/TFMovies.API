using Microsoft.AspNetCore.Mvc;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Utils;

public static class ModelStateErrorResponseFactory
{
    public static IActionResult GenerateErrorResponse(ActionContext context)
    {
        var errors = context
                    .ModelState
                    .Values
                    .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    .Where(errorMsg => !string.IsNullOrEmpty(errorMsg))
                    .ToList();

        var errorResponse = new ErrorResponse
        {
            ErrorMessage = string.Join(" ", errors),
            ErrorDetails = "Errors captured by InvalidModelStateResponseFactory."
        };

        return new BadRequestObjectResult(errorResponse);
    }
}
