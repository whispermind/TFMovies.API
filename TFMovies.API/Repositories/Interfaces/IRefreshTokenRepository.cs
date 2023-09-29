using TFMovies.API.Data.Entities;

namespace TFMovies.API.Repositories.Interfaces;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{    
    public Task<RefreshToken?> FindByTokenAsync(string token);
}
