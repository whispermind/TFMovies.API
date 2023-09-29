using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Common.Constants;
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
    ///     
    /// </remarks>
    [HttpPost("login")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(LoginResponse))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(404, "NOT_FOUND", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest model)
    {
        var callBackUrl = GenerateVerifyEmailUrl();

        var response = await _userService.LoginAsync(model, callBackUrl);

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
    ///     
    /// </remarks>
    [HttpPost("logout")]
    [Authorize]
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
        var response = await _userService.RefreshJwtTokens(model);

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
    /// 
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
    public async Task<IActionResult> VerifyEmailAsync([FromBody] EmailVerifyRequest model)
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
    public async Task<IActionResult> SendActivationEmailAsync([FromBody] EmailActivateRequest model)
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
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] PasswordForgotRequest model)
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
    public async Task<IActionResult> ValidateResetTokenAsync([FromBody] ResetTokenValidateRequest model)
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
    public async Task<IActionResult> ResetPasswordAsync([FromBody] PasswordResetRequest model)
    {
        await _userService.ResetPasswordAsync(model);

        return Ok();
    }

    /// <summary>
    /// Changes the role of the currently authenticated user.
    /// </summary>
    /// <param name="newRole">The new role to assign to the user.</param>
    /// <returns>Status 204 if successful.</returns>
    /// <remarks>
    /// Example:
    /// 
    ///     PUT /users/change-role
    ///     {
    ///        "newRole": "Author"
    ///     }
    /// 
    /// Note: The role change will only be successful if the user is authorized and the specified role exists.
    /// </remarks>        
    [HttpPut("change-role")]
    [Authorize]
    [SwaggerOperation(Tags = new[] { "Helpers" })]
    [SwaggerResponse(204, "NO_CONTENT")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> ChangeRoleAsync(string newRole)
    {
        await _userService.ChangeRoleAsync(newRole, User);

        return NoContent();
    }

    /// <summary>
    /// Retrieves the list of top authors.
    /// </summary>   
    /// <param name="model">Contains optional parameters for limit, sort, and order of the returned records. The default values are: sort = "rated", order = "desc", and limit = 1.</param>
    /// <returns>Returns status 200 along with the list of top authors if the operation is successful.</returns>
    /// <remarks>
    /// Example of a GET request to retrieve top authors:
    ///
    ///     GET /users/authors?limit=3
    ///     
    /// </remarks>
    [HttpGet("authors")]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(IEnumerable<UserShortInfoDto>))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> GetAuthorsAsync([FromQuery] PagingSortParams model)
    {
        var result = await _userService.GetAuthorsAsync(model);

        return Ok(result);
    }


    /// <summary>
    /// Retrieves a paginated list of users based on the provided search and filter criteria.
    /// </summary>
    /// <param name="pagingSortModel">
    /// A model containing pagination, sorting, and filtering parameters:
    /// - **page**: The page number. (Optional; default is 1)
    /// - **limit**: The maximum number of users to retrieve. (Optional; default is 100)
    /// - **sort**: The field by which to sort the users ("email" or "role" or "nickname"(default)). (Optional)
    /// - **order**: The order in which to sort the users ("asc"(default) or "desc"). (Optional)    
    /// </param>
    /// <param name="filterModel">
    /// The search and filter criteria:
    /// - **roleId**: A specific role ID to sort the user by. (Optional)
    /// </param>
    /// <param name="queryModel">
    /// The search and filter criteria:
    /// - **users**: Search terms used to filter the users by email or nickname. (Optional)   
    /// </param>
    /// <returns>Returns a status of 200 along with a paginated list of users that match the search and filter criteria.</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /users?page=1&amp;limit=10&amp;sort=created&amp;order=desc&amp;roleId=roleId
    ///     GET /users?page=1&amp;limit=10&amp;users=sample,sample1
    ///
    /// **Note**: This endpoint can be accessed by any user. However, the search functionality is available only for authorized users. 
    /// </remarks>   
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Author + "," + RoleNames.User)]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(UsersPaginatedResponse))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] PagingSortParams pagingSortModel,
        [FromQuery] UsersFilterParams filterModel,
        [FromQuery] UsersQueryParams queryModel)
    {
        var result = await _userService.GetAllPagingAsync(pagingSortModel, filterModel, queryModel, User);

        return Ok(result);
    }

    private string GenerateVerifyEmailUrl() => $"{ExtractOriginOrDefault()}/signup";
    private string GenerateValidateResetTokenUrl() => $"{ExtractOriginOrDefault()}/auth/passrecovery";

    private string ExtractOriginOrDefault()
    {
        if (Request.Headers.TryGetValue("Origin", out var originValues) && originValues.Count > 0)
        {
            return originValues[0]?.ToString() ?? _webConfig.DefaultSiteUrl ?? "FallbackDefaultValue";
        }
        else
        {
            return _webConfig.DefaultSiteUrl ?? "FallbackDefaultValue";
        }
    }
}
