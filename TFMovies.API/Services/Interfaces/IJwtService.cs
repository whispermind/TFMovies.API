using TFMovies.API.Data.Entities;

namespace TFMovies.API.Services.Interfaces;

public interface IJwtService
{
    public string GenerateAccessToken(User user, IEnumerable<string> userRoles);
    public RefreshToken GenerateRefreshTokenAsync();      
}
