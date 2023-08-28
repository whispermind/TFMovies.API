using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IUserAuthenticationService
{
    public ValueTask<JwtTokensResponse> LoginAsync(UserLoginRequest model, string callBackUrl);
    public Task<JwtTokensResponse> RefreshJwtTokens(RefreshTokenRequest model);
}
