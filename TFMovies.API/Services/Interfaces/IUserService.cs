using System.Security.Claims;
using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IUserService
{
    public Task<LoginResponse> LoginAsync(LoginRequest model, string callBackUrl);
    public Task LogoutAsync(LogoutRequest model);
    public Task<JwtTokensResponse> RefreshJwtTokens(RefreshTokenRequest model);    
    public Task RegisterAsync(RegisterRequest model, string callBackUrl);
    public Task VerifyEmailAsync(EmailVerifyRequest model);
    public Task SendActivationEmailAsync(EmailActivateRequest model, string callBackUrl);
    public Task ForgotPasswordAsync(PasswordForgotRequest model, string callBackUrl);
    public Task<UserActionToken> ValidateResetTokenAsync(string token, bool setUsed);
    public Task ResetPasswordAsync(PasswordResetRequest model);    
    public Task ChangeRoleAsync(string id, ChangeRoleRequest model);
    public Task<IEnumerable<UserShortInfoDto>> GetAuthorsAsync(PagingSortParams model);
    public Task<PagedResult<UserShortInfoDto>> GetAllPagingAsync(PagingSortParams pagingSortModel, UsersFilterParams filterModel, UsersQueryParams queryModel, ClaimsPrincipal currentUserPrincipal);
    public Task SoftDeleteAsync(string id);
}
