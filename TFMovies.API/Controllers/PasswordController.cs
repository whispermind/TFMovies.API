using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Controllers;

[Route("tfmovies/password")]
[ApiController]
[Produces("application/json")]
public class PasswordController : ControllerBase
{
    private readonly IUserPasswordService _userPasswordService;
    private readonly UrlSettings _urlSettings;
    public PasswordController(IUserPasswordService userPasswordService, IOptions<UrlSettings> urlSettings)
    {
        _userPasswordService = userPasswordService;
        _urlSettings = urlSettings.Value;
    }

    /// <summary>
    /// Sends an email to the user with a link to confirm password recovery.
    /// </summary>
    /// <param name="model">The request model containing the user email.</param>
    /// <returns>200 if the email is successfully verified, otherwise returns an appropriate error status.</returns>
    /// <remarks>
    /// Example:
    ///
    ///     POST /tfmovies/password/forgot
    ///     {
    ///         "email": "john.doe@example.com"         
    ///     }
    ///
    /// </remarks>
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    [HttpPost("forgot")]
    public async Task<IActionResult> SendVerificationLinkToEmailAsync([FromBody] LinkWithTokenRequest model)
    {
        var callBackUrl = GenerateCallBackUrl();

        await _userPasswordService.SendResetPasswordLinkToEmailAsync(model.Email, callBackUrl);
        
        return Ok();
    }

    /// <summary>
    /// Confirms the validity of the password reset token.
    /// </summary>
    /// <param name="model">The token for password recovery.</param>
    /// <returns>Status 200 if the token is successfully verified, otherwise returns an appropriate error status.</returns>
    /// <remarks>
    /// Example:
    ///     
    ///     POST /tfmovies/password/verify-token
    ///     {     
    ///       "token": "sample_reset_token_string"
    ///     }     
    ///
    /// </remarks>  
    [HttpPost("verify-token", Name = "VerifyResetToken")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> VerifyResetTokenAsync([FromBody] VerifySecretTokenRequest model)
    {
        await _userPasswordService.VerifyResetTokenAsync(model.Token, false);

        return Ok();
    }

    /// <summary>
    /// Initiates the password recovery process.
    /// </summary>
    /// <param name="model">The request containing the user's email, reset token and the new password details.</param>
    /// <returns>Status 200 if the password recovery is successful, otherwise returns an appropriate error status.</returns>
    /// <remarks>
    /// Example:
    ///     
    ///     POST /tfmovies/auth/recovery
    ///     {
    ///       "email": "user@example.com",
    ///       "token": "sample_reset_token_string",
    ///       "newPassword": "34Jvqt+K",
    ///       "confirmPassword": "34Jvqt+K"
    ///     }     
    ///
    /// </remarks>  
    [HttpPost("recovery")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(404, "NOT_FOUND", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> RecoveryPasswordAsync([FromBody] RecoveryPasswordRequest model)
    {
        await _userPasswordService.RecoveryPasswordAsync(model);

        return Ok();
    }

    private string GenerateCallBackUrl() => $"{_urlSettings.Domain}{Url.RouteUrl("VerifyResetToken")}";
}
