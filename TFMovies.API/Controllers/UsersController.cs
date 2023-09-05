using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Controllers;


[Route("users")]
[ApiController]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly WebConfig _webConfig;

    public UsersController(IUserService userServicee, IOptions<WebConfig> webConfig)
    {
        _userService = userServicee;
        _webConfig = webConfig.Value;
    }

    /// <summary>
    /// User login.
    /// </summary>
    /// <param name="model">User login credentials.</param>
    /// <returns>Status 200 and info of current user and the pair of Access-Refresh tokens if successful.</returns>
    /// <remarks>
    /// Example:
    ///
    ///     POST /users/login
    ///     {
    ///       "email": "john.doe@example.com",
    ///       "password": "34Jvqt+K"
    ///     }
    /// </remarks>
    [HttpPost("login")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(LoginResponse))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(404, "NOT_FOUND", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest model)
    {
        var callBackUrl = GenerateVerifyEmailUrl();

        var ipAddress = IpAddress();

        var response = await _userService.LoginAsync(model, callBackUrl, ipAddress);       

        return Ok(response);
    }

    /// <summary>
    /// User logout.
    /// </summary>
    /// <param name="model">Access and Refresh Tokens.</param>
    /// <returns>Status 200 if successful.</returns>
    /// <remarks>
    /// Example:
    ///
    ///     POST /users/logout
    ///     {
    ///       "accessToken": "current_access_token",
    ///       "refreshToken": "current_refresh_token"
    ///     }
    /// </remarks>
    [HttpPost("logout")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]   
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> LogoutAsync([FromBody] LogoutRequest model)
    {
        await _userService.LogoutAsync(model);

        return Ok();
    }

    /// <summary>
    /// Refreshes the JWT tokens using a valid refresh token.
    /// </summary>
    /// <param name="model">The request model containing the access and refresh tokens.</param>
    /// <returns>A new pair of Access and Refresh tokens if successful, otherwise returns an appropriate error status.</returns>
    /// <remarks>
    /// Example:
    ///
    ///     POST /users/refresh-token
    ///     {         
    ///         "refreshToken": "current_refresh_token"
    ///     }
    ///
    /// </remarks>
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(JwtTokensResponse))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]        
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshTokensAsync([FromBody] RefreshTokenRequest model)
    {
        var ipAddress = IpAddress();

        var response = await _userService.RefreshJwtTokens(model, ipAddress);      

        return Ok(response);
    }

    

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="model">The user registration details.</param>
    /// <returns>Status 201 if successful.</returns>
    /// <remarks>
    /// Example:
    /// 
    /// POST /users/signup
    /// {
    ///   "nickname": "Jonny",
    ///   "email": "john.doe@example.com",
    ///   "password": "34Jvqt+K",
    ///   "confirmPassword": "34Jvqt+K"
    /// }
    /// </remarks>
    [HttpPost("signup")]
    [SwaggerResponse(201, "CREATED")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(409, "CONFLICT", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest model)
    {
        var callBackUrl = GenerateVerifyEmailUrl();

        await _userService.RegisterAsync(model, callBackUrl);

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
    ///     POST /users/verify-email
    ///     {     
    ///       "token": "sample_confirmation_token_string"
    ///     }     
    ///
    /// </remarks>  
    [HttpPost("verify-email", Name = "VerifyEmail")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> VerifyEmailAsync([FromBody] VerifyEmailRequest model)
    {
        await _userService.VerifyEmailAsync(model);

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
    ///      POST /users/send-activation-email
    ///     {     
    ///       "email": "john.doe@example.com"
    ///     }
    ///
    /// </remarks>   
    [HttpPost("send-activation-email")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(404, "NOT_FOUND", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> SendActivationEmailAsync([FromBody] ActivateEmailRequest model)
    {
        var callBackUrl = GenerateVerifyEmailUrl();

        await _userService.SendActivationEmailAsync(model, callBackUrl);

        return Ok();
    }

    /// <summary>
    /// Sends an email to the user with a link to confirm password recovery.
    /// </summary>
    /// <param name="model">The request model containing the user email.</param>
    /// <returns>200 if the email is successfully verified, otherwise returns an appropriate error status.</returns>
    /// <remarks>
    /// Example:
    ///
    ///     POST /users/forgot-password
    ///     {
    ///         "email": "john.doe@example.com"         
    ///     }
    ///
    /// </remarks>
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest model)
    {
        var callBackUrl = GenerateValidateResetTokenUrl();

        await _userService.ForgotPasswordAsync(model, callBackUrl);

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
    ///     POST /users/validate-reset-token
    ///     {     
    ///       "token": "sample_reset_token_string"
    ///     }     
    ///
    /// </remarks>  
    [HttpPost("validate-reset-token", Name = "ValidateResetToken")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> ValidateResetTokenAsync([FromBody] ValidateResetTokenRequest model)
    {
        await _userService.ValidateResetTokenAsync(model.Token, false);

        return Ok();
    }

    /// <summary>
    /// Initiates the password recovery process.
    /// </summary>
    /// <param name="model">The request containing the reset token and the new password details.</param>
    /// <returns>Status 200 if the password recovery is successful, otherwise returns an appropriate error status.</returns>
    /// <remarks>
    /// Example:
    ///     
    ///     POST /users/reset-password
    ///     {  
    ///       "token": "sample_reset_token_string",
    ///       "newPassword": "34Jvqt+K",
    ///       "confirmPassword": "34Jvqt+K"
    ///     }     
    ///
    /// </remarks>  
    [HttpPost("reset-password")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(404, "NOT_FOUND", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest model)
    {
        await _userService.ResetPasswordAsync(model);

        return Ok();
    }

    // helper methods
    private string IpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? string.Empty;
        else
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
    }
    
    private string GenerateVerifyEmailUrl() => $"{ExtractOriginOrDefault()}/signup";
    private string GenerateValidateResetTokenUrl() => $"{ExtractOriginOrDefault()}/passrecovery";

    private string ExtractOriginOrDefault()
    {
        if (Request.Headers.TryGetValue("Origin", out var originValues) && originValues.Count > 0)
        {
            return originValues[0].ToString();
        }
        else
        {
            return _webConfig.DefaultSiteUrl;
        }
    }
}
