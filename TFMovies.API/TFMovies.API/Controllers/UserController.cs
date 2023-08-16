using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Controllers;

[ApiController]
[Route("tfmovies/user")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;   
    private readonly UrlSettings _urlSettings;

    public UserController(IUserService userService, IUserSecretTokenService userSecretTokenService, IOptions<UrlSettings> urlSettings)
    {
        _userService = userService;       
        _urlSettings = urlSettings.Value;
    }


    /// <summary>
    /// Verifies the user's email based on the provided token.
    /// </summary>
    /// <param name="model">The request model containing the secret token for email confirmation.</param>
    /// <returns>Status 200 if the email is successfully verified, otherwise returns an appropriate error status.</returns>
    /// <remarks>
    /// Example:
    ///
    ///     POST /tfmovies/user/confirm-email
    ///     {
    ///         "Token": "your_secret_token_here"
    ///     }
    ///
    /// </remarks>  
    [HttpPost("confirm-email")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(404, "NOT_FOUND", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> ConfirmEmailByTokenAsync([FromBody]ConfirmEmailRequest model)
    {
        await _userService.ConfirmEmailByTokenAsync(model.Token);

        return Ok();
    }

    /// <summary>
    /// Sends an email to the user with a link to verify their email address.
    /// </summary>
    /// <param name="email">The email address to which the link will be sent.</param>
    /// <returns>Status 200 if the email was sent successfully, otherwise returns an appropriate error status.</returns>
    /// <remarks>
    /// Example:
    ///
    ///     GET /tfmovies/user/new-confirm-link?email=your_email@example.com
    ///
    /// </remarks>   
    [HttpGet("new-confirm-link")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(404, "NOT_FOUND", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]    
    public async Task<IActionResult> SendConfirmLinkViaEmailAsync(string email)
    {
        var callBackUrl = $"{_urlSettings.Domain}{Url.Action("ConfirmEmailByTokenAsync", "User")}";

        await _userService.RequestNewConfirmationEmailAsync(email, callBackUrl);        

        return Ok();
    }    
}
