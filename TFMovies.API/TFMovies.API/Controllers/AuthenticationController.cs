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
    private readonly IUserService _userService;
    private readonly UrlSettings _urlSettings;

    public AuthenticationController(IUserService userService, IOptions<UrlSettings> urlSettings)
    {
        _userService = userService;
        _urlSettings = urlSettings.Value;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="model">The user registration details.</param>
    /// <returns>Status 201 if successful.</returns>
    /// <remarks>
    /// Example:
    /// 
    /// POST /tfmovies/auth/register
    /// {
    ///   "Nickname": "Jonny",
    ///   "Email": "john.doe@example.com",
    ///   "Password": "34Jvqt+K",
    ///   "ConfirmPassword": "34Jvqt+K"
    /// }
    /// </remarks>
    [HttpPost("register")]
    [SwaggerResponse(201, "CREATED")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(409, "CONFLICT", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> RegisterAsync([FromBody] UserRegisterRequest model)
    {        
        var callBackUrl = $"{_urlSettings.Domain}{Url.Action("ConfirmEmailByTokenAsync", "User")}";

        await _userService.RegisterAsync(model, callBackUrl);

        return StatusCode(StatusCodes.Status201Created);
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
    ///       "Email": "john.doe@example.com",
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

        var response = await _userService.LoginAsync(model);

        return Ok(response);
    }
}
