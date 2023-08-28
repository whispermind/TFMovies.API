using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Controllers;

[Route("tfmovies/signup")]
[ApiController]
[Produces("application/json")]
public class RegistrationController : ControllerBase
{
    private readonly IUserRegistrationService _userRegistrationService;   
    private readonly UrlSettings _urlSettings;

    public RegistrationController(IUserRegistrationService userRegistrationService, IOptions<UrlSettings> urlSettings)
    {
        _userRegistrationService = userRegistrationService;       
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
    /// POST /tfmovies/signup
    /// {
    ///   "nickname": "Jonny",
    ///   "email": "john.doe@example.com",
    ///   "password": "34Jvqt+K",
    ///   "confirmPassword": "34Jvqt+K"
    /// }
    /// </remarks>
    [HttpPost]
    [SwaggerResponse(201, "CREATED")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(409, "CONFLICT", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> RegisterAsync([FromBody] UserRegisterRequest model)
    {
        var callBackUrl = GenerateCallBackUrl();

        await _userRegistrationService.RegisterAsync(model, callBackUrl);

        return StatusCode(StatusCodes.Status201Created);
    }


    /// <summary>
    /// Verifies the user's email based on the provided token.
    /// </summary>
    /// <param name="model">The token for email confirmation.</param>
    /// <returns>Status 200 if the email is successfully verified, otherwise returns an appropriate error status.</returns>
    /// <remarks>
    /// Example:
    ///     
    ///     POST /tfmovies/signup/verify-token
    ///     {     
    ///       "token": "sample_confirmation_token_string"
    ///     }     
    ///
    /// </remarks>  
    [HttpPost("verify-token", Name = "VerifyEmailByToken")]    
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]    
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> ConfirmEmailByTokenAsync([FromBody] VerifySecretTokenRequest model)
    {
        await _userRegistrationService.ConfirmEmailByTokenAsync(model.Token);

        return Ok();
    }


    /// <summary>
    /// Sends an email to the user with a link to verify their email address.
    /// </summary>
    /// <param name="model">The email address to which the link will be sent.</param>
    /// <returns>Status 200 if the email was sent successfully, otherwise returns an appropriate error status.</returns>
    /// <remarks>
    /// Example:
    ///    
    ///      POST /tfmovies/signup/send-confirm-link
    ///     {     
    ///       "email": "john.doe@example.com"
    ///     }
    ///
    /// </remarks>   
    [HttpPost("send-confirm-link")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(404, "NOT_FOUND", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]    
    public async Task<IActionResult> SendConfirmationLinkToEmailAsync([FromBody] LinkWithTokenRequest model)
    {
        var callBackUrl = GenerateCallBackUrl();

        await _userRegistrationService.SendConfirmationLinkToEmailAsync(model.Email, callBackUrl);        

        return Ok();
    }
    
    private string GenerateCallBackUrl() => $"{_urlSettings.Domain}{Url.RouteUrl("VerifyEmailByToken")}";
}
