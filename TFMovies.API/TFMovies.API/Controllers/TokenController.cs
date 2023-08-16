using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Controllers;


[Route("tfmovies/token")]
[ApiController]
[Produces("application/json")]
public class TokenController : ControllerBase
{
    private readonly IJwtService _jwtService;
    public TokenController(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }

    /// <summary>
    /// Refreshes the JWT tokens using a valid refresh token.
    /// </summary>
    /// <param name="model">The request model containing the access and refresh tokens.</param>
    /// <returns>A new pair of Access and Refresh tokens if successful, otherwise returns an appropriate error status.</returns>
    /// <remarks>
    /// Example:
    ///
    ///     POST /tfmovies/token/refresh
    ///     {
    ///         "AccessToken": "your_current_access_token",
    ///         "RefreshToken": "your_current_refresh_token"
    ///     }
    ///
    /// </remarks>
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(JwtTokensResponse))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(403, "FORBIDDEN")]
    [SwaggerResponse(409, "CONFLICT", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshTokensAsync([FromBody]RefreshTokenRequest model)
    {
        var result = await _jwtService.RefreshTokensAsync(model);

        return Ok(result);
    }
}
