using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Controllers;


[Route("tfmovies/auth")]
[ApiController]
[Produces("application/json")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly UrlSettings _urlSettings;

    public AuthenticationController(IUserAuthenticationService userAuthenticationService, IOptions<UrlSettings> urlSettings)
    {
        _userAuthenticationService = userAuthenticationService;
        _urlSettings = urlSettings.Value;
    }

    /// <summary>
    /// User login.
    /// </summary>
    /// <param name="model">User login credentials.</param>
    /// <returns>Status 200 and the pair of Access-Refresh tokens if successful.</returns>
    /// <remarks>
    /// Example:
    ///
    ///     POST /tfmovies/auth/login
    ///     {
    ///       "email": "john.doe@example.com",
    ///       "Password": "34Jvqt+K"
    ///     }
    /// </remarks>
    [HttpPost("login")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(JwtTokensResponse))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(404, "NOT_FOUND", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> LoginAsync([FromBody] UserLoginRequest model)
    {
        var callBackUrl = GenerateCallBackUrl();

        var response = await _userAuthenticationService.LoginAsync(model, callBackUrl);

        return Ok(response);
    }

    /// <summary>
    /// Refreshes the JWT tokens using a valid refresh token.
    /// </summary>
    /// <param name="model">The request model containing the access and refresh tokens.</param>
    /// <returns>A new pair of Access and Refresh tokens if successful, otherwise returns an appropriate error status.</returns>
    /// <remarks>
    /// Example:
    ///
    ///     POST /tfmovies/auth/refresh-token
    ///     {
    ///         "accessToken": "current_access_token",
    ///         "refreshToken": "current_refresh_token"
    ///     }
    ///
    /// </remarks>
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(JwtTokensResponse))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(403, "FORBIDDEN")]
    [SwaggerResponse(409, "CONFLICT", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshTokensAsync([FromBody] RefreshTokenRequest model)
    {
        var result = await _userAuthenticationService.RefreshJwtTokens(model);
        return Ok(result);
    }
    
    private string GenerateCallBackUrl() => $"{_urlSettings.Domain}{Url.RouteUrl("VerifyEmailByToken")}";
}
