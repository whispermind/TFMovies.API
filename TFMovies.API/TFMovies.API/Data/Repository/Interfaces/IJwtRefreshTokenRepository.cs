using TFMovies.API.Data.Entities;

namespace TFMovies.API.Data.Repository.Interfaces;

public interface IJwtRefreshTokenRepository : IBaseRepository<JwtRefreshToken>
{
    public Task<JwtRefreshToken?> GetActiveTokenAsync(string token);
}
