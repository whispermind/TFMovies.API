using System.Security.Claims;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IJwtService
{
    public string GenerateAccessToken(IEnumerable<Claim> claims);
    public Task<string> GenerateRefreshToken(string userId);
    public Task<JwtTokensResponse> RefreshTokensAsync(RefreshTokenRequest model);
}
