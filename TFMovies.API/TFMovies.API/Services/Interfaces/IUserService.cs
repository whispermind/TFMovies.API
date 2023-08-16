using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IUserService
{
    public Task RegisterAsync(UserRegisterRequest model, string callBackUrl);
    public ValueTask<JwtTokensResponse> LoginAsync(UserLoginRequest model);    
    public Task RequestNewConfirmationEmailAsync(string email, string callbackUrl);    
    public Task ConfirmEmailByTokenAsync(string token);
}
